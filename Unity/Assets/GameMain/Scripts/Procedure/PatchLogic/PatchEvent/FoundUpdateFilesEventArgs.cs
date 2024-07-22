using GameFramework;
using GameFramework.Event;

namespace UnityGameFramework.Runtime
{
    public class FoundUpdateFilesEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(FoundUpdateFilesEventArgs).GetHashCode();
        public override int Id => EventId;

        public int TotalCount { get; private set; }
        public long TotalSizeBytes { get; private set; }

        public static FoundUpdateFilesEventArgs Create(int totalDownloadCount, long totalDownloadBytes)
        {
            var eventArgs = ReferencePool.Acquire<FoundUpdateFilesEventArgs>();
            eventArgs.TotalCount = totalDownloadCount;
            eventArgs.TotalSizeBytes = totalDownloadBytes;
            return eventArgs;
        }

        public override void Clear()
        {
            TotalCount = 0;
            TotalSizeBytes = 0;
        }
    }
}