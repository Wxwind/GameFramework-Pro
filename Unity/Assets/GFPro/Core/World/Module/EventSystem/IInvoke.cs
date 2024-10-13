using System;

namespace GFPro
{
    public interface IInvoke
    {
        Type Type { get; }
    }

    public abstract class AInvokeHandler<A> : IInvoke where A : struct
    {
        public Type Type => typeof(A);

        public abstract void Handle(A a);
    }

    public abstract class AInvokeHandler<A, T> : IInvoke where A : struct
    {
        public Type Type => typeof(A);

        public abstract T Handle(A a);
    }
}