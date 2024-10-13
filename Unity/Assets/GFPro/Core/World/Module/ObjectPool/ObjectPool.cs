using System;
using System.Collections.Generic;

namespace GFPro
{
    public class ObjectPool : Singleton<ObjectPool>, ISingletonAwake
    {
        private Dictionary<Type, Queue<object>> pool;

        public void Awake()
        {
            pool = new Dictionary<Type, Queue<object>>();
        }

        public T Fetch<T>() where T : class
        {
            return Fetch(typeof(T)) as T;
        }

        public object Fetch(Type type)
        {
            Queue<object> queue = null;
            if (!pool.TryGetValue(type, out queue))
            {
                return Activator.CreateInstance(type);
            }

            if (queue.Count == 0)
            {
                return Activator.CreateInstance(type);
            }

            return queue.Dequeue();
        }

        public void Recycle(object obj)
        {
            var type = obj.GetType();
            Queue<object> queue = null;
            if (!pool.TryGetValue(type, out queue))
            {
                queue = new Queue<object>();
                pool.Add(type, queue);
            }

            // 一种对象最大为1000个
            if (queue.Count > 1000)
            {
                return;
            }

            queue.Enqueue(obj);
        }
    }
}