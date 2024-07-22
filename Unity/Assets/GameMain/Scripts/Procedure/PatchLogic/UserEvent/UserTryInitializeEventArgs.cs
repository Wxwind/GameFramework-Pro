using GameFramework;
using GameFramework.Event;

namespace UnityGameFramework.Runtime
{
    public class UserTryInitializeEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(UserTryInitializeEventArgs).GetHashCode();
        public override int Id => EventId;

        public static UserTryInitializeEventArgs Create()
        {
            var eventArgs = ReferencePool.Acquire<UserTryInitializeEventArgs>();

            return eventArgs;
        }

        public override void Clear()
        {
        }
    }
}