using GameFramework;
using GameFramework.Event;

namespace UnityGameFramework.Runtime
{
    public sealed class PatchStatesChangeEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(PatchStatesChangeEventArgs).GetHashCode();
        public override int Id => EventId;

        public string Tips { get; private set; }

        public static PatchStatesChangeEventArgs Create(string message)
        {
            var eventArgs = ReferencePool.Acquire<PatchStatesChangeEventArgs>();
            eventArgs.Tips = message;
            return eventArgs;
        }

        public override void Clear()
        {
            Tips = null;
        }
    }
}