namespace GFPro
{
    [ComponentOf(typeof(Session))]
    public class SessionIdleCheckerComponent : Entity, IAwake, IDestroy
    {
        public long RepeatedTimer;
    }
}