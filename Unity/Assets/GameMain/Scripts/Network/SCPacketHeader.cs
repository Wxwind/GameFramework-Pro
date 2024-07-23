namespace GameMain
{
    public sealed class SCPacketHeader : PacketHeaderBase
    {
        public override PacketType PacketType => PacketType.ServerToClient;
    }
}