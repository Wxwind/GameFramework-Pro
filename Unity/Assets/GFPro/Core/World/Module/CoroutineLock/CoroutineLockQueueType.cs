using System.Collections.Generic;

namespace GFPro
{
    public class CoroutineLockQueueType
    {
        private readonly int type;

        private readonly Dictionary<long, CoroutineLockQueue> coroutineLockQueues = new();

        public CoroutineLockQueueType(int type)
        {
            this.type = type;
        }

        private CoroutineLockQueue Get(long key)
        {
            coroutineLockQueues.TryGetValue(key, out var queue);
            return queue;
        }

        private CoroutineLockQueue New(long key)
        {
            var queue = CoroutineLockQueue.Create(type, key);
            coroutineLockQueues.Add(key, queue);
            return queue;
        }

        private void Remove(long key)
        {
            if (coroutineLockQueues.Remove(key, out var queue))
            {
                queue.Recycle();
            }
        }

        public async ETTask<CoroutineLock> Wait(long key, int time)
        {
            var queue = Get(key) ?? New(key);
            return await queue.Wait(time);
        }

        public void Notify(long key, int level)
        {
            var queue = Get(key);
            if (queue == null)
            {
                return;
            }

            if (!queue.Notify(level))
            {
                Remove(key);
            }
        }
    }
}