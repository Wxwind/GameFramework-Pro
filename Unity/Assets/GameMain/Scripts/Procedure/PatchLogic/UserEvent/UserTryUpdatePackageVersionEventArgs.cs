using GameFramework;
using GameFramework.Event;

namespace UnityGameFramework.Runtime
{
    public class UserTryUpdatePackageVersionEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(UserTryUpdatePackageVersionEventArgs).GetHashCode();
        public override int Id => EventId;

        public static UserTryUpdatePackageVersionEventArgs Create()
        {
            var eventArgs = ReferencePool.Acquire<UserTryUpdatePackageVersionEventArgs>();

            return eventArgs;
        }

        public override void Clear()
        {
        }
    }
}