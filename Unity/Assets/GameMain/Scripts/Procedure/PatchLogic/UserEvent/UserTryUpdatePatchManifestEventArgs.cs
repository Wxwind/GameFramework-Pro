using GameFramework;
using GameFramework.Event;

namespace UnityGameFramework.Runtime
{
    public class UserTryUpdatePatchManifestEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(UserTryUpdatePatchManifestEventArgs).GetHashCode();
        public override int Id => EventId;

        public static UserTryUpdatePatchManifestEventArgs Create()
        {
            var eventArgs = ReferencePool.Acquire<UserTryUpdatePatchManifestEventArgs>();

            return eventArgs;
        }

        public override void Clear()
        {
        }
    }
}