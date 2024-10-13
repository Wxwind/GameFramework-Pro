using System;

namespace GFPro
{
    public interface IUpdate
    {
        void Update();
    }

    public interface IUpdateSystem : ISystemType
    {
        void Run(Entity o);
    }

    [ObjectSystem]
    public abstract class UpdateSystem<T> : IUpdateSystem where T : Entity, IUpdate
    {
        void IUpdateSystem.Run(Entity o)
        {
            Update((T)o);
        }

        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUpdateSystem);
        }

        InstanceQueueIndex ISystemType.GetInstanceQueueIndex()
        {
            return InstanceQueueIndex.Update;
        }

        protected abstract void Update(T self);
    }
}