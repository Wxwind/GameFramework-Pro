using System.Collections.Generic;

namespace GFPro
{
    public class CoroutineLockQueue
    {
        private int  type;
        private long key;

        public static CoroutineLockQueue Create(int type, long key)
        {
            var coroutineLockQueue = ObjectPool.Instance.Fetch<CoroutineLockQueue>();
            coroutineLockQueue.type = type;
            coroutineLockQueue.key = key;
            return coroutineLockQueue;
        }

        private CoroutineLock currentCoroutineLock;

        private readonly Queue<WaitCoroutineLock> queue = new();

        public int Count => queue.Count;

        public async ETTask<CoroutineLock> Wait(int time)
        {
            if (currentCoroutineLock == null)
            {
                currentCoroutineLock = CoroutineLock.Create(type, key, 1);
                return currentCoroutineLock;
            }

            var waitCoroutineLock = WaitCoroutineLock.Create();
            queue.Enqueue(waitCoroutineLock);
            if (time > 0)
            {
                var tillTime = TimeInfo.Instance.ClientFrameTime() + time;
                TimerComponent.Instance.NewOnceTimer(tillTime, TimerCoreInvokeType.CoroutineTimeout, waitCoroutineLock);
            }

            currentCoroutineLock = await waitCoroutineLock.Wait();
            return currentCoroutineLock;
        }

        // 返回值，有没有成功执行下一个协程锁
        public bool Notify(int level)
        {
            // 有可能WaitCoroutineLock已经超时抛出异常，所以要找到一个未处理的WaitCoroutineLock
            while (queue.Count > 0)
            {
                var waitCoroutineLock = queue.Dequeue();
                if (waitCoroutineLock.IsDisposed())
                {
                    continue;
                }

                var coroutineLock = CoroutineLock.Create(type, key, level);

                waitCoroutineLock.SetResult(coroutineLock);
                return true;
            }

            return false;
        }

        public void Recycle()
        {
            queue.Clear();
            key = 0;
            type = 0;
            currentCoroutineLock = null;
            ObjectPool.Instance.Recycle(this);
        }
    }
}