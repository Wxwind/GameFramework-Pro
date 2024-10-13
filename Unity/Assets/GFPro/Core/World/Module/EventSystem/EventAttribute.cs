using System;

namespace GFPro
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class EventAttribute : BaseAttribute
    {
        public SceneType SceneType { get; }

        public EventAttribute(SceneType sceneType)
        {
            SceneType = sceneType;
        }
    }
}