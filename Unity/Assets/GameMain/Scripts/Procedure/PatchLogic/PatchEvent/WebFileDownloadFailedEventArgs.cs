using GameFramework;
using GameFramework.Event;

namespace UnityGameFramework.Runtime
{
    public class WebFileDownloadFailedEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(WebFileDownloadFailedEventArgs).GetHashCode();
        public override int Id => EventId;

        public string FileName { get; private set; }
        public string Error { get; private set; }

        public static WebFileDownloadFailedEventArgs Create(string fileName, string error)
        {
            var eventArgs = ReferencePool.Acquire<WebFileDownloadFailedEventArgs>();
            eventArgs.FileName = fileName;
            eventArgs.Error = error;
            return eventArgs;
        }

        public override void Clear()
        {
            FileName = null;
            Error = null;
        }
    }
}