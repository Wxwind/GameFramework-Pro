using System;

namespace GFPro
{
    public class CoroutineLock : IDisposable
    {
        private int  type;
        private long key;
        private int  level;

        public static CoroutineLock Create(int type, long k, int count)
        {
            var coroutineLock = ObjectPool.Instance.Fetch<CoroutineLock>();
            coroutineLock.type = type;
            coroutineLock.key = k;
            coroutineLock.level = count;
            return coroutineLock;
        }

        public void Dispose()
        {
            CoroutineLockComponent.Instance.RunNextCoroutine(type, key, level + 1);

            type = CoroutineLockType.None;
            key = 0;
            level = 0;

            ObjectPool.Instance.Recycle(this);
        }
    }
}