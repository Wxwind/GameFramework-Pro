using System;

namespace GFPro
{
    public interface ISingleton : IDisposable
    {
        void Register();
        void Destroy();
        bool IsDisposed();
    }

    public abstract class Singleton<T> : ISingleton where T : Singleton<T>, new()
    {
        private        bool isDisposed;
        private static T    instance;

        public static T Instance => instance;

        void ISingleton.Register()
        {
            if (instance != null)
            {
                throw new Exception($"singleton register twice! {typeof(T).Name}");
            }

            instance = (T)this;
        }

        void ISingleton.Destroy()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;

            instance.Dispose();
            instance = null;
        }

        bool ISingleton.IsDisposed()
        {
            return isDisposed;
        }

        public virtual void Dispose()
        {
        }
    }
}