using System;
using System.Collections.Generic;

namespace GFPro
{
    public class ListComponent<T> : List<T>, IDisposable
    {
        public ListComponent()
        {
        }

        public static ListComponent<T> Create()
        {
            return ObjectPool.Instance.Fetch(typeof(ListComponent<T>)) as ListComponent<T>;
        }

        public void Dispose()
        {
            if (Capacity > 64) // 超过64，让gc回收
            {
                return;
            }

            Clear();
            ObjectPool.Instance.Recycle(this);
        }
    }
}