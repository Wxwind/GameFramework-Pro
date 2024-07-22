using GameFramework;
using GameFramework.Event;

namespace UnityGameFramework.Runtime
{
    public class DownloadProgressUpdateEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(WebFileDownloadFailedEventArgs).GetHashCode();
        public override int Id => EventId;

        public int TotalDownloadCount { get; private set; }
        public int CurrentDownloadCount { get; private set; }
        public long TotalDownloadSizeBytes { get; private set; }
        public long CurrentDownloadSizeBytes { get; private set; }

        public static DownloadProgressUpdateEventArgs Create(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)
        {
            var eventArgs = ReferencePool.Acquire<DownloadProgressUpdateEventArgs>();
            eventArgs.CurrentDownloadCount = currentDownloadCount;
            eventArgs.CurrentDownloadSizeBytes = currentDownloadBytes;
            eventArgs.TotalDownloadCount = totalDownloadCount;
            eventArgs.TotalDownloadSizeBytes = totalDownloadBytes;
            return eventArgs;
        }

        public override void Clear()
        {
            TotalDownloadCount = 0;
            TotalDownloadSizeBytes = 0;
            CurrentDownloadSizeBytes = 0;
            CurrentDownloadCount = 0;
        }
    }
}