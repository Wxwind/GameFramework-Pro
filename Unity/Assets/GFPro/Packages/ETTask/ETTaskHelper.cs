using System.Collections.Generic;

namespace GFPro
{
    public static class ETTaskHelper
    {
        public static bool IsCancel(this ETCancellationToken self)
        {
            if (self == null)
            {
                return false;
            }

            return self.IsDispose();
        }

        private class CoroutineBlocker
        {
            private int count;

            private ETTask tcs;

            public CoroutineBlocker(int count)
            {
                this.count = count;
            }

            public async ETTask RunSubCoroutineAsync(ETTask task)
            {
                try
                {
                    await task;
                }
                finally
                {
                    --count;

                    if (count <= 0 && tcs != null)
                    {
                        var t = tcs;
                        tcs = null;
                        t.SetResult();
                    }
                }
            }

            public async ETTask WaitAsync()
            {
                if (count <= 0)
                {
                    return;
                }

                tcs = ETTask.Create(true);
                await tcs;
            }
        }

        public static async ETTask WaitAny(List<ETTask> tasks)
        {
            if (tasks.Count == 0)
            {
                return;
            }

            var coroutineBlocker = new CoroutineBlocker(1);

            foreach (var task in tasks)
            {
                coroutineBlocker.RunSubCoroutineAsync(task).Coroutine();
            }

            await coroutineBlocker.WaitAsync();
        }

        public static async ETTask WaitAny(ETTask[] tasks)
        {
            if (tasks.Length == 0)
            {
                return;
            }

            var coroutineBlocker = new CoroutineBlocker(1);

            foreach (var task in tasks)
            {
                coroutineBlocker.RunSubCoroutineAsync(task).Coroutine();
            }

            await coroutineBlocker.WaitAsync();
        }

        public static async ETTask WaitAll(ETTask[] tasks)
        {
            if (tasks.Length == 0)
            {
                return;
            }

            var coroutineBlocker = new CoroutineBlocker(tasks.Length);

            foreach (var task in tasks)
            {
                coroutineBlocker.RunSubCoroutineAsync(task).Coroutine();
            }

            await coroutineBlocker.WaitAsync();
        }

        public static async ETTask WaitAll(List<ETTask> tasks)
        {
            if (tasks.Count == 0)
            {
                return;
            }

            var coroutineBlocker = new CoroutineBlocker(tasks.Count);

            foreach (var task in tasks)
            {
                coroutineBlocker.RunSubCoroutineAsync(task).Coroutine();
            }

            await coroutineBlocker.WaitAsync();
        }
    }
}