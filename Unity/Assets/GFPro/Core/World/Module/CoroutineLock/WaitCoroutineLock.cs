using System;

namespace GFPro
{
    [Invoke(TimerCoreInvokeType.CoroutineTimeout)]
    public class WaitCoroutineLockTimer : ATimer<WaitCoroutineLock>
    {
        protected override void Run(WaitCoroutineLock waitCoroutineLock)
        {
            if (waitCoroutineLock.IsDisposed())
            {
                return;
            }

            waitCoroutineLock.SetException(new Exception("coroutine is timeout!"));
        }
    }

    public class WaitCoroutineLock
    {
        public static WaitCoroutineLock Create()
        {
            var waitCoroutineLock = new WaitCoroutineLock();
            waitCoroutineLock.tcs = ETTask<CoroutineLock>.Create(true);
            return waitCoroutineLock;
        }

        private ETTask<CoroutineLock> tcs;

        public void SetResult(CoroutineLock coroutineLock)
        {
            if (tcs == null)
            {
                throw new NullReferenceException("SetResult tcs is null");
            }

            var t = tcs;
            tcs = null;
            t.SetResult(coroutineLock);
        }

        public void SetException(Exception exception)
        {
            if (tcs == null)
            {
                throw new NullReferenceException("SetException tcs is null");
            }

            var t = tcs;
            tcs = null;
            t.SetException(exception);
        }

        public bool IsDisposed()
        {
            return tcs == null;
        }

        public async ETTask<CoroutineLock> Wait()
        {
            return await tcs;
        }
    }
}