using System;
using ProtoBuf;

namespace GameMain
{
    [Serializable]
    [ProtoContract(Name = @"CSHeartBeat")]
    public class CSHeartBeat : CSPacketBase
    {
        public override int Id => 1;

        public override void Clear()
        {
        }
    }
}