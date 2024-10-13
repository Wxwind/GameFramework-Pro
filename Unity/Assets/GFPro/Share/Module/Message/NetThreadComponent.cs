using System.Threading;

namespace GFPro
{
    [ComponentOf(typeof(Scene))]
    public class NetThreadComponent : Entity, IAwake, ILateUpdate, IDestroy
    {
        public static NetThreadComponent Instance;

        public Thread thread;
        public bool   isStop;
    }
}