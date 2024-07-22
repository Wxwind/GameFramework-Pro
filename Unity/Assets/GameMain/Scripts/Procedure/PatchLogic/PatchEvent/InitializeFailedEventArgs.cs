using GameFramework;
using GameFramework.Event;

namespace UnityGameFramework.Runtime
{
    public sealed class InitializeFailedEventArgs : GameEventArgs
    {
      
        public static readonly int EventId = typeof(InitializeFailedEventArgs).GetHashCode();
        public override int Id => EventId;
        
        public static InitializeFailedEventArgs Create()
        {
            var eventArgs = ReferencePool.Acquire<InitializeFailedEventArgs>();
        
            return eventArgs;
        }

        public override void Clear()
        {
            
        }
    }
}
