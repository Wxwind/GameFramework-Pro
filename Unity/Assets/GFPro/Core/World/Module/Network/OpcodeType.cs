using System;
using System.Collections.Generic;

namespace GFPro
{
    public class OpcodeType : Singleton<OpcodeType>, ISingletonAwake
    {
        // 初始化后不变，所以主线程，网络线程都可以读
        private readonly DoubleMap<Type, ushort> typeOpcode = new();

        private readonly Dictionary<Type, Type> requestResponse = new();

        public void Awake()
        {
            HashSet<Type> types = CodeTypes.Instance.GetTypes(typeof(MessageAttribute));
            foreach (var type in types)
            {
                var att = type.GetCustomAttributes(typeof(MessageAttribute), false);
                if (att.Length == 0)
                {
                    continue;
                }

                var messageAttribute = att[0] as MessageAttribute;
                if (messageAttribute == null)
                {
                    continue;
                }

                var opcode = messageAttribute.Opcode;
                if (opcode != 0)
                {
                    typeOpcode.Add(type, opcode);
                }

                // 检查request response
                if (typeof(IRequest).IsAssignableFrom(type))
                {
                    if (typeof(ILocationMessage).IsAssignableFrom(type))
                    {
                        requestResponse.Add(type, typeof(MessageResponse));
                        continue;
                    }

                    var attrs = type.GetCustomAttributes(typeof(ResponseTypeAttribute), false);
                    if (attrs.Length == 0)
                    {
                        Log.Error($"not found responseType: {type}");
                        continue;
                    }

                    var responseTypeAttribute = attrs[0] as ResponseTypeAttribute;
                    requestResponse.Add(type, CodeTypes.Instance.GetType($"GFPro.{responseTypeAttribute.Type}"));
                }
            }
        }

        public ushort GetOpcode(Type type)
        {
            return typeOpcode.GetValueByKey(type);
        }

        public Type GetType(ushort opcode)
        {
            var type = typeOpcode.GetKeyByValue(opcode);
            if (type == null)
            {
                throw new Exception($"OpcodeType not found type: {opcode}");
            }

            return type;
        }

        public Type GetResponseType(Type request)
        {
            if (!requestResponse.TryGetValue(request, out var response))
            {
                throw new Exception($"not found response type, request type: {request.FullName}");
            }

            return response;
        }
    }
}