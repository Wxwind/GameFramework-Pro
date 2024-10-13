using System;
using System.Collections.Generic;

namespace GFPro
{
    public class MultiMap<T, K> : SortedDictionary<T, List<K>>
    {
        private readonly List<K>        Empty = new();
        private readonly int            maxPoolCount;
        private readonly Queue<List<K>> pool;

        public MultiMap(int maxPoolCount = 0)
        {
            this.maxPoolCount = maxPoolCount;
            pool = new Queue<List<K>>(maxPoolCount);
        }

        private List<K> FetchList()
        {
            if (pool.Count > 0)
            {
                return pool.Dequeue();
            }

            return new List<K>(10);
        }

        private void Recycle(List<K> list)
        {
            if (list == null)
            {
                return;
            }

            if (pool.Count == maxPoolCount)
            {
                return;
            }

            list.Clear();
            pool.Enqueue(list);
        }

        public void Add(T t, K k)
        {
            List<K> list;
            TryGetValue(t, out list);
            if (list == null)
            {
                list = FetchList();
                Add(t, list);
            }

            list.Add(k);
        }

        public bool Remove(T t, K k)
        {
            List<K> list;
            TryGetValue(t, out list);
            if (list == null)
            {
                return false;
            }

            if (!list.Remove(k))
            {
                return false;
            }

            if (list.Count == 0)
            {
                Remove(t);
            }

            return true;
        }

        public new bool Remove(T t)
        {
            List<K> list;
            TryGetValue(t, out list);
            if (list == null)
            {
                return false;
            }

            Recycle(list);
            return base.Remove(t);
        }

        /// <summary>
        /// 不返回内部的list,copy一份出来
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public K[] GetAll(T t)
        {
            List<K> list;
            TryGetValue(t, out list);
            if (list == null)
            {
                return Array.Empty<K>();
            }

            return list.ToArray();
        }

        /// <summary>
        /// 返回内部的list
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public new List<K> this[T t]
        {
            get
            {
                TryGetValue(t, out var list);
                return list ?? Empty;
            }
        }

        public K GetOne(T t)
        {
            List<K> list;
            TryGetValue(t, out list);
            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return default;
        }

        public bool Contains(T t, K k)
        {
            List<K> list;
            TryGetValue(t, out list);
            if (list == null)
            {
                return false;
            }

            return list.Contains(k);
        }
    }
}