using GameFramework;
using GameFramework.Network;

namespace GameMain
{
    public abstract class PacketHeaderBase : IPacketHeader, IReference
    {
        public abstract PacketType PacketType { get; }

        public int Id { get; set; }

        public bool IsValid => PacketType != PacketType.Undefined && Id > 0 && PacketLength >= 0;

        public int PacketLength { get; set; }

        public void Clear()
        {
            Id = 0;
            PacketLength = 0;
        }
    }
}