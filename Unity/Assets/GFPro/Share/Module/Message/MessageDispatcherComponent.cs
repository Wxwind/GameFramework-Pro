using System.Collections.Generic;

namespace GFPro
{
    public class MessageDispatcherInfo
    {
        public SceneType SceneType { get; }
        public IMessageSessionHandler IMHandler { get; }

        public MessageDispatcherInfo(SceneType sceneType, IMessageSessionHandler imHandler)
        {
            SceneType = sceneType;
            IMHandler = imHandler;
        }
    }

    /// <summary>
    /// 消息分发组件
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class MessageDispatcherComponent : Entity, IAwake, IDestroy, ILoad
    {
        public static MessageDispatcherComponent Instance { get; set; }

        public readonly Dictionary<ushort, List<MessageDispatcherInfo>> Handlers = new();
    }
}