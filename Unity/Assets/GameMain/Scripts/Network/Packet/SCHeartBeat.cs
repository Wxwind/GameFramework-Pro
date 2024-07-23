using System;
using ProtoBuf;

namespace GameMain
{
    [Serializable]
    [ProtoContract(Name = @"SCHeartBeat")]
    public class SCHeartBeat : SCPacketBase
    {
        public override int Id => 2;

        public override void Clear()
        {
        }
    }
}