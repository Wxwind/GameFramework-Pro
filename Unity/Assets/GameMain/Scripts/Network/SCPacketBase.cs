namespace GameMain
{
    public abstract class SCPacketBase : PacketBase
    {
        public override PacketType PacketType => PacketType.ServerToClient;
    }
}