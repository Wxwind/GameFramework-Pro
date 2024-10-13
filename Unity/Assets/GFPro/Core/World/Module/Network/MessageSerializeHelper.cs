using System;
using System.IO;

namespace GFPro
{
    public static class MessageSerializeHelper
    {
        public static byte[] Serialize(MessageObject message)
        {
            return MemoryPackHelper.Serialize(message);
        }

        public static void Serialize(MessageObject message, MemoryBuffer stream)
        {
            MemoryPackHelper.Serialize(message, stream);
        }

        public static MessageObject Deserialize(Type type, byte[] bytes, int index, int count)
        {
            var o = ObjectPool.Instance.Fetch(type);
            MemoryPackHelper.Deserialize(type, bytes, index, count, ref o);
            return o as MessageObject;
        }

        public static MessageObject Deserialize(Type type, MemoryBuffer stream)
        {
            var o = ObjectPool.Instance.Fetch(type);
            MemoryPackHelper.Deserialize(type, stream, ref o);
            return o as MessageObject;
        }

        public static ushort MessageToStream(MemoryBuffer stream, MessageObject message, int headOffset = 0)
        {
            var opcode = OpcodeType.Instance.GetOpcode(message.GetType());

            stream.Seek(headOffset + Packet.OpcodeLength, SeekOrigin.Begin);
            stream.SetLength(headOffset + Packet.OpcodeLength);

            stream.GetBuffer().WriteTo(headOffset, opcode);

            Serialize(message, stream);

            stream.Seek(0, SeekOrigin.Begin);
            return opcode;
        }

        public static (ushort, MemoryBuffer) ToMemoryBuffer(AService service, long actorId, object message)
        {
            var memoryBuffer = service.Fetch();
            ushort opcode = 0;
            switch (service.ServiceType)
            {
                case ServiceType.Inner:
                {
                    opcode = MessageToStream(memoryBuffer, (MessageObject)message, Packet.ActorIdLength);
                    memoryBuffer.GetBuffer().WriteTo(0, actorId);
                    break;
                }
                case ServiceType.Outer:
                {
                    opcode = MessageToStream(memoryBuffer, (MessageObject)message);
                    break;
                }
            }

            ((MessageObject)message).Dispose(); // 回收message

            return (opcode, memoryBuffer);
        }

        public static (long, object) ToMessage(AService service, MemoryBuffer memoryStream)
        {
            object message = null;
            long actorId = default;
            switch (service.ServiceType)
            {
                case ServiceType.Outer:
                {
                    memoryStream.Seek(Packet.OpcodeLength, SeekOrigin.Begin);
                    var opcode = BitConverter.ToUInt16(memoryStream.GetBuffer(), 0);
                    var type = OpcodeType.Instance.GetType(opcode);
                    message = Deserialize(type, memoryStream);
                    break;
                }
                case ServiceType.Inner:
                {
                    memoryStream.Seek(Packet.ActorIdLength + Packet.OpcodeLength, SeekOrigin.Begin);
                    var buffer = memoryStream.GetBuffer();
                    actorId = BitConverter.ToInt64(buffer, Packet.ActorIdIndex);
                    var opcode = BitConverter.ToUInt16(buffer, Packet.ActorIdLength);
                    var type = OpcodeType.Instance.GetType(opcode);
                    message = Deserialize(type, memoryStream);
                    break;
                }
            }

            service.Recycle(memoryStream);

            return (actorId, message);
        }
    }
}