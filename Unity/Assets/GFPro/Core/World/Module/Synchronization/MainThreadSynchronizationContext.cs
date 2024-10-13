using System;
using System.Threading;

namespace GFPro
{
    public class MainThreadSynchronizationContext : Singleton<MainThreadSynchronizationContext>, ISingletonAwake, ISingletonUpdate
    {
        private readonly ThreadSynchronizationContext threadSynchronizationContext = new();

        public void Awake()
        {
        }

        public MainThreadSynchronizationContext()
        {
            SynchronizationContext.SetSynchronizationContext(threadSynchronizationContext);
        }

        public void Update()
        {
            threadSynchronizationContext.Update();
        }

        public void Post(SendOrPostCallback callback, object state)
        {
            Post(() => callback(state));
        }

        public void Post(Action action)
        {
            threadSynchronizationContext.Post(action);
        }
    }
}