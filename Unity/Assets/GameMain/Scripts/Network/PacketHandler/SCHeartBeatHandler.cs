using GameFramework.Network;
using UnityGameFramework.Runtime;

namespace GameMain
{
    public class SCHeartBeatHandler : PacketHandlerBase
    {
        public override int Id => 2;

        public override void Handle(object sender, Packet packet)
        {
            var packetImpl = (SCHeartBeat)packet;
            Log.Info("Receive packet '{0}'.", packetImpl.Id.ToString());
        }
    }
}