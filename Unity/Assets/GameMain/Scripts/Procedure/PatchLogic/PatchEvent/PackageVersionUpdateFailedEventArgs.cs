using GameFramework;
using GameFramework.Event;

namespace UnityGameFramework.Runtime
{
    public sealed class PackageVersionUpdateFailedEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(PackageVersionUpdateFailedEventArgs).GetHashCode();
        public override int Id => EventId;

        public static PackageVersionUpdateFailedEventArgs Create()
        {
            var eventArgs = ReferencePool.Acquire<PackageVersionUpdateFailedEventArgs>();

            return eventArgs;
        }

        public override void Clear()
        {
        }
    }
}