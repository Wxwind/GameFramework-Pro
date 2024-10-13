using System;
using System.Collections.Generic;

namespace GFPro
{
    public static class OpcodeTypeComponentSystem
    {
        [ObjectSystem]
        public class OpcodeTypeComponentAwakeSystem : AwakeSystem<OpcodeTypeComponent>
        {
            protected override void Awake(OpcodeTypeComponent self)
            {
                OpcodeTypeComponent.Instance = self;

                self.requestResponse.Clear();

                var types = EventSystem.Instance.GetTypes(typeof(MessageAttribute));
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

                    if (OpcodeHelper.IsOuterMessage(opcode) && typeof(IActorMessage).IsAssignableFrom(type))
                    {
                        self.outrActorMessage.Add(opcode);
                    }

                    // 检查request response
                    if (typeof(IRequest).IsAssignableFrom(type))
                    {
                        if (typeof(IActorLocationMessage).IsAssignableFrom(type))
                        {
                            self.requestResponse.Add(type, typeof(ActorResponse));
                            continue;
                        }

                        var attrs = type.GetCustomAttributes(typeof(ResponseTypeAttribute), false);
                        if (attrs.Length == 0)
                        {
                            Log.Error($"not found responseType: {type}");
                            continue;
                        }

                        var responseTypeAttribute = attrs[0] as ResponseTypeAttribute;
                        self.requestResponse.Add(type, EventSystem.Instance.GetType($"ET.{responseTypeAttribute.Type}"));
                    }
                }
            }
        }

        [ObjectSystem]
        public class OpcodeTypeComponentDestroySystem : DestroySystem<OpcodeTypeComponent>
        {
            protected override void Destroy(OpcodeTypeComponent self)
            {
                OpcodeTypeComponent.Instance = null;
            }
        }

        public static bool IsOutrActorMessage(this OpcodeTypeComponent self, ushort opcode)
        {
            return self.outrActorMessage.Contains(opcode);
        }

        public static Type GetResponseType(this OpcodeTypeComponent self, Type request)
        {
            if (!self.requestResponse.TryGetValue(request, out var response))
            {
                throw new Exception($"not found response type, request type: {request.GetType().Name}");
            }

            return response;
        }
    }

    [ComponentOf(typeof(Scene))]
    public class OpcodeTypeComponent : Entity, IAwake, IDestroy
    {
        [StaticField] public static OpcodeTypeComponent Instance;

        public HashSet<ushort> outrActorMessage = new();

        public readonly Dictionary<Type, Type> requestResponse = new();
    }
}