using System;

namespace GFPro
{
    public interface ISystemType
    {
        Type Type();
        Type SystemType();
        InstanceQueueIndex GetInstanceQueueIndex();
    }
}