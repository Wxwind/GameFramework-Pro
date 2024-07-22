using GameFramework;
using GameFramework.Event;

namespace UnityGameFramework.Runtime
{
    public class UserBeginDownloadWebFilesEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(UserBeginDownloadWebFilesEventArgs).GetHashCode();
        public override int Id => EventId;

        public static UserBeginDownloadWebFilesEventArgs Create()
        {
            var eventArgs = ReferencePool.Acquire<UserBeginDownloadWebFilesEventArgs>();

            return eventArgs;
        }

        public override void Clear()
        {
        }
    }
}