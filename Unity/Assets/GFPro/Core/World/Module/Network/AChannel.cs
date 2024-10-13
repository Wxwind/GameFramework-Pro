using System;
using System.IO;
using System.Net;

namespace GFPro
{
    public enum ChannelType
    {
        Connect,
        Accept
    }

    public struct Packet
    {
        public const int MinPacketSize = 2;
        public const int OpcodeLength  = 2;
        public const int ActorIdIndex  = 0;
        public const int ActorIdLength = 8;

        public ushort       Opcode;
        public long         ActorId;
        public MemoryStream MemoryStream;
    }

    public abstract class AChannel : IDisposable
    {
        public long Id;

        public ChannelType ChannelType { get; protected set; }

        public int Error { get; set; }

        private IPEndPoint remoteAddress;

        public IPEndPoint RemoteAddress
        {
            get => remoteAddress;
            set => remoteAddress = value;
        }

        public bool IsDisposed => Id == 0;

        public abstract void Dispose();
    }
}