using System;
using System.Collections.Generic;

namespace GFPro
{
    public class World
    {
        private static World instance;

        public static World Instance
        {
            get { return instance ??= new World(); }
        }

        private readonly Dictionary<Type, ISingleton> singletonTypes  = new();
        private readonly Stack<ISingleton>            singletons      = new();
        private readonly Queue<ISingleton>            updates         = new();
        private readonly Queue<ISingleton>            lateUpdates     = new();
        private readonly Queue<ETTask>                frameFinishTask = new();

        public T AddSingleton<T>() where T : Singleton<T>, ISingletonAwake, new()
        {
            T singleton = new();
            singleton.Awake();

            AddSingleton(singleton);
            return singleton;
        }

        public T AddSingleton<T, A>(A a) where T : Singleton<T>, ISingletonAwake<A>, new()
        {
            T singleton = new();
            singleton.Awake(a);

            AddSingleton(singleton);
            return singleton;
        }

        public T AddSingleton<T, A, B>(A a, B b) where T : Singleton<T>, ISingletonAwake<A, B>, new()
        {
            T singleton = new();
            singleton.Awake(a, b);

            AddSingleton(singleton);
            return singleton;
        }

        public T AddSingleton<T, A, B, C>(A a, B b, C c) where T : Singleton<T>, ISingletonAwake<A, B, C>, new()
        {
            T singleton = new();
            singleton.Awake(a, b, c);

            AddSingleton(singleton);
            return singleton;
        }

        public void AddSingleton(ISingleton singleton)
        {
            var singletonType = singleton.GetType();
            if (singletonTypes.ContainsKey(singletonType))
            {
                throw new Exception($"already exist singleton: {singletonType.Name}");
            }

            singletonTypes.Add(singletonType, singleton);
            singletons.Push(singleton);

            singleton.Register();

            if (singleton is ISingletonAwake awake)
            {
                awake.Awake();
            }

            if (singleton is ISingletonUpdate)
            {
                updates.Enqueue(singleton);
            }

            if (singleton is ISingletonLateUpdate)
            {
                lateUpdates.Enqueue(singleton);
            }
        }

        public async ETTask WaitFrameFinish()
        {
            var task = ETTask.Create(true);
            frameFinishTask.Enqueue(task);
            await task;
        }

        public void Update()
        {
            var count = updates.Count;
            while (count-- > 0)
            {
                var singleton = updates.Dequeue();

                if (singleton.IsDisposed())
                {
                    continue;
                }

                if (singleton is not ISingletonUpdate update)
                {
                    continue;
                }

                updates.Enqueue(singleton);
                try
                {
                    update.Update();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void LateUpdate()
        {
            var count = lateUpdates.Count;
            while (count-- > 0)
            {
                var singleton = lateUpdates.Dequeue();

                if (singleton.IsDisposed())
                {
                    continue;
                }

                if (singleton is not ISingletonLateUpdate lateUpdate)
                {
                    continue;
                }

                lateUpdates.Enqueue(singleton);
                try
                {
                    lateUpdate.LateUpdate();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void FrameFinishUpdate()
        {
            while (frameFinishTask.Count > 0)
            {
                var task = frameFinishTask.Dequeue();
                task.SetResult();
            }
        }

        public void Close()
        {
            // 顺序反过来清理
            while (singletons.Count > 0)
            {
                var iSingleton = singletons.Pop();
                iSingleton.Destroy();
            }

            singletonTypes.Clear();
        }
    }
}