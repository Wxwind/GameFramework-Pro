namespace GFPro
{
    public class MessageHandlerAttribute : BaseAttribute
    {
        public SceneType SceneType { get; }

        public MessageHandlerAttribute(SceneType sceneType)
        {
            SceneType = sceneType;
        }
    }
}