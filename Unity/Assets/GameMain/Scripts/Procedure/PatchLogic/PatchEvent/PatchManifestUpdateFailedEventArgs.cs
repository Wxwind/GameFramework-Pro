using GameFramework;
using GameFramework.Event;

namespace UnityGameFramework.Runtime
{
    public sealed class PatchManifestUpdateFailedEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(PatchManifestUpdateFailedEventArgs).GetHashCode();
        public override int Id => EventId;


        public static PatchManifestUpdateFailedEventArgs Create()
        {
            var eventArgs = ReferencePool.Acquire<PatchManifestUpdateFailedEventArgs>();

            return eventArgs;
        }

        public override void Clear()
        {
        }
    }
}