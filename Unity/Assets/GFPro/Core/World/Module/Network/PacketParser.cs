using System;
using System.IO;

namespace GFPro
{
    public enum ParserState
    {
        PacketSize,
        PacketBody
    }

    public class PacketParser
    {
        private readonly CircularBuffer buffer;
        private          int            packetSize;
        private          ParserState    state;
        private readonly AService       service;
        private readonly byte[]         cache                 = new byte[8];
        public const     int            InnerPacketSizeLength = 4;
        public const     int            OuterPacketSizeLength = 2;
        public           MemoryBuffer   MemoryBuffer;

        public PacketParser(CircularBuffer buffer, AService service)
        {
            this.buffer = buffer;
            this.service = service;
        }

        public bool Parse(out MemoryBuffer memoryBuffer)
        {
            while (true)
            {
                switch (state)
                {
                    case ParserState.PacketSize:
                    {
                        if (service.ServiceType == ServiceType.Inner)
                        {
                            if (buffer.Length < InnerPacketSizeLength)
                            {
                                memoryBuffer = null;
                                return false;
                            }

                            buffer.Read(cache, 0, InnerPacketSizeLength);

                            packetSize = BitConverter.ToInt32(cache, 0);
                            if (packetSize > ushort.MaxValue * 16 || packetSize < Packet.MinPacketSize)
                            {
                                throw new Exception($"recv packet size error, 可能是外网探测端口: {packetSize}");
                            }
                        }
                        else
                        {
                            if (buffer.Length < OuterPacketSizeLength)
                            {
                                memoryBuffer = null;
                                return false;
                            }

                            buffer.Read(cache, 0, OuterPacketSizeLength);

                            packetSize = BitConverter.ToUInt16(cache, 0);
                            if (packetSize < Packet.MinPacketSize)
                            {
                                throw new Exception($"recv packet size error, 可能是外网探测端口: {packetSize}");
                            }
                        }

                        state = ParserState.PacketBody;
                        break;
                    }
                    case ParserState.PacketBody:
                    {
                        if (buffer.Length < packetSize)
                        {
                            memoryBuffer = null;
                            return false;
                        }

                        memoryBuffer = service.Fetch(packetSize);
                        buffer.Read(memoryBuffer, packetSize);
                        //memoryStream.SetLength(this.packetSize - Packet.MessageIndex);

                        memoryBuffer.Seek(0, SeekOrigin.Begin);

                        state = ParserState.PacketSize;
                        return true;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}