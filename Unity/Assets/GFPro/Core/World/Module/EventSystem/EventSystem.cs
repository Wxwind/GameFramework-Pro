using System;
using System.Collections.Generic;

namespace GFPro
{
    public class EventSystem : Singleton<EventSystem>, ISingletonAwake, ISingletonUpdate, ISingletonLateUpdate
    {
        private class OneTypeSystems
        {
            public readonly UnOrderMultiMap<Type, object> Map = new();

            // 这里不用hash，数量比较少，直接for循环速度更快
            public readonly bool[] QueueFlag = new bool[(int)InstanceQueueIndex.Max];
        }

        private class TypeSystems
        {
            private readonly Dictionary<Type, OneTypeSystems> typeSystemsMap = new();

            public OneTypeSystems GetOrCreateOneTypeSystems(Type type)
            {
                OneTypeSystems systems = null;
                typeSystemsMap.TryGetValue(type, out systems);
                if (systems != null)
                {
                    return systems;
                }

                systems = new OneTypeSystems();
                typeSystemsMap.Add(type, systems);
                return systems;
            }

            public OneTypeSystems GetOneTypeSystems(Type type)
            {
                OneTypeSystems systems = null;
                typeSystemsMap.TryGetValue(type, out systems);
                return systems;
            }

            public List<object> GetSystems(Type type, Type systemType)
            {
                OneTypeSystems oneTypeSystems = null;
                if (!typeSystemsMap.TryGetValue(type, out oneTypeSystems))
                {
                    return null;
                }

                if (!oneTypeSystems.Map.TryGetValue(systemType, out var systems))
                {
                    return null;
                }

                return systems;
            }
        }

        private class EventInfo
        {
            public IEvent IEvent { get; }

            public SceneType SceneType { get; }

            public EventInfo(IEvent iEvent, SceneType sceneType)
            {
                IEvent = iEvent;
                SceneType = sceneType;
            }
        }

        private readonly Dictionary<string, Type> allTypes = new();

        private readonly UnOrderMultiMapSet<Type, Type> types = new();

        private readonly Dictionary<Type, List<EventInfo>> allEvents = new();

        private Dictionary<Type, Dictionary<int, object>> allInvokes = new();

        private TypeSystems typeSystems = new();

        private readonly Queue<long>[] queues = new Queue<long>[(int)InstanceQueueIndex.Max];


        public void Awake()
        {
            for (var i = 0; i < queues.Length; i++)
            {
                queues[i] = new Queue<long>();
            }
        }


        public void Add(Dictionary<string, Type> addTypes)
        {
            allTypes.Clear();
            types.Clear();

            foreach (var (fullName, type) in addTypes)
            {
                allTypes[fullName] = type;

                if (type.IsAbstract)
                {
                    continue;
                }

                // 记录所有的有BaseAttribute标记的的类型
                var objects = type.GetCustomAttributes(typeof(BaseAttribute), true);

                foreach (var o in objects)
                {
                    types.Add(o.GetType(), type);
                }
            }

            typeSystems = new TypeSystems();

            foreach (var type in GetTypes(typeof(ObjectSystemAttribute)))
            {
                var obj = Activator.CreateInstance(type);

                if (obj is ISystemType iSystemType)
                {
                    var oneTypeSystems = typeSystems.GetOrCreateOneTypeSystems(iSystemType.Type());
                    oneTypeSystems.Map.Add(iSystemType.SystemType(), obj);
                    var index = iSystemType.GetInstanceQueueIndex();
                    if (index > InstanceQueueIndex.None && index < InstanceQueueIndex.Max)
                    {
                        oneTypeSystems.QueueFlag[(int)index] = true;
                    }
                }
            }

            allEvents.Clear();
            foreach (var type in types[typeof(EventAttribute)])
            {
                var obj = Activator.CreateInstance(type) as IEvent;
                if (obj == null)
                {
                    throw new Exception($"type not is AEvent: {type.Name}");
                }

                var attrs = type.GetCustomAttributes(typeof(EventAttribute), false);
                foreach (var attr in attrs)
                {
                    var eventAttribute = attr as EventAttribute;

                    var eventType = obj.Type;

                    EventInfo eventInfo = new(obj, eventAttribute.SceneType);

                    if (!allEvents.ContainsKey(eventType))
                    {
                        allEvents.Add(eventType, new List<EventInfo>());
                    }

                    allEvents[eventType].Add(eventInfo);
                }
            }

            allInvokes = new Dictionary<Type, Dictionary<int, object>>();
            foreach (var type in types[typeof(InvokeAttribute)])
            {
                var obj = Activator.CreateInstance(type);
                var iInvoke = obj as IInvoke;
                if (iInvoke == null)
                {
                    throw new Exception($"type not is callback: {type.Name}");
                }

                var attrs = type.GetCustomAttributes(typeof(InvokeAttribute), false);
                foreach (var attr in attrs)
                {
                    if (!allInvokes.TryGetValue(iInvoke.Type, out var dict))
                    {
                        dict = new Dictionary<int, object>();
                        allInvokes.Add(iInvoke.Type, dict);
                    }

                    var invokeAttribute = attr as InvokeAttribute;

                    try
                    {
                        dict.Add(invokeAttribute.Type, obj);
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"action type duplicate: {iInvoke.Type.Name} {invokeAttribute.Type}", e);
                    }
                }
            }
        }

        public HashSet<Type> GetTypes(Type systemAttributeType)
        {
            if (!types.ContainsKey(systemAttributeType))
            {
                return new HashSet<Type>();
            }

            return types[systemAttributeType];
        }

        public Dictionary<string, Type> GetTypes()
        {
            return allTypes;
        }

        public Type GetType(string typeName)
        {
            return allTypes[typeName];
        }

        public void RegisterSystem(Entity component)
        {
            var type = component.GetType();

            var oneTypeSystems = typeSystems.GetOneTypeSystems(type);
            if (oneTypeSystems == null)
            {
                return;
            }

            for (var i = 0; i < oneTypeSystems.QueueFlag.Length; ++i)
            {
                if (!oneTypeSystems.QueueFlag[i])
                {
                    continue;
                }

                queues[i].Enqueue(component.InstanceId);
            }
        }

        public void Deserialize(Entity component)
        {
            var iDeserializeSystems = typeSystems.GetSystems(component.GetType(), typeof(IDeserializeSystem));
            if (iDeserializeSystems == null)
            {
                return;
            }

            foreach (IDeserializeSystem deserializeSystem in iDeserializeSystems)
            {
                if (deserializeSystem == null)
                {
                    continue;
                }

                try
                {
                    deserializeSystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        // GetComponentSystem
        public void GetComponent(Entity entity, Entity component)
        {
            var iGetSystem = typeSystems.GetSystems(entity.GetType(), typeof(IGetComponentSystem));
            if (iGetSystem == null)
            {
                return;
            }

            foreach (IGetComponentSystem getSystem in iGetSystem)
            {
                if (getSystem == null)
                {
                    continue;
                }

                try
                {
                    getSystem.Run(entity, component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        // AddComponentSystem
        public void AddComponent(Entity entity, Entity component)
        {
            var iAddSystem = typeSystems.GetSystems(entity.GetType(), typeof(IAddComponentSystem));
            if (iAddSystem == null)
            {
                return;
            }

            foreach (IAddComponentSystem addComponentSystem in iAddSystem)
            {
                if (addComponentSystem == null)
                {
                    continue;
                }

                try
                {
                    addComponentSystem.Run(entity, component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Awake(Entity component)
        {
            var iAwakeSystems = typeSystems.GetSystems(component.GetType(), typeof(IAwakeSystem));
            if (iAwakeSystems == null)
            {
                return;
            }

            foreach (IAwakeSystem aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Awake<P1>(Entity component, P1 p1)
        {
            var iAwakeSystems = typeSystems.GetSystems(component.GetType(), typeof(IAwakeSystem<P1>));
            if (iAwakeSystems == null)
            {
                return;
            }

            foreach (IAwakeSystem<P1> aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Awake<P1, P2>(Entity component, P1 p1, P2 p2)
        {
            var iAwakeSystems = typeSystems.GetSystems(component.GetType(), typeof(IAwakeSystem<P1, P2>));
            if (iAwakeSystems == null)
            {
                return;
            }

            foreach (IAwakeSystem<P1, P2> aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1, p2);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Awake<P1, P2, P3>(Entity component, P1 p1, P2 p2, P3 p3)
        {
            var iAwakeSystems = typeSystems.GetSystems(component.GetType(), typeof(IAwakeSystem<P1, P2, P3>));
            if (iAwakeSystems == null)
            {
                return;
            }

            foreach (IAwakeSystem<P1, P2, P3> aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1, p2, p3);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Awake<P1, P2, P3, P4>(Entity component, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            var iAwakeSystems = typeSystems.GetSystems(component.GetType(), typeof(IAwakeSystem<P1, P2, P3, P4>));
            if (iAwakeSystems == null)
            {
                return;
            }

            foreach (IAwakeSystem<P1, P2, P3, P4> aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1, p2, p3, p4);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Load()
        {
            var queue = queues[(int)InstanceQueueIndex.Load];
            var count = queue.Count;
            while (count-- > 0)
            {
                var instanceId = queue.Dequeue();
                var component = Root.Instance.Get(instanceId);
                if (component == null)
                {
                    continue;
                }

                if (component.IsDisposed)
                {
                    continue;
                }

                var iLoadSystems = typeSystems.GetSystems(component.GetType(), typeof(ILoadSystem));
                if (iLoadSystems == null)
                {
                    continue;
                }

                queue.Enqueue(instanceId);

                foreach (ILoadSystem iLoadSystem in iLoadSystems)
                {
                    try
                    {
                        iLoadSystem.Run(component);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }
        }

        public void Destroy(Entity component)
        {
            var iDestroySystems = typeSystems.GetSystems(component.GetType(), typeof(IDestroySystem));
            if (iDestroySystems == null)
            {
                return;
            }

            foreach (IDestroySystem iDestroySystem in iDestroySystems)
            {
                if (iDestroySystem == null)
                {
                    continue;
                }

                try
                {
                    iDestroySystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Update()
        {
            var queue = queues[(int)InstanceQueueIndex.Update];
            var count = queue.Count;
            while (count-- > 0)
            {
                var instanceId = queue.Dequeue();
                var component = Root.Instance.Get(instanceId);
                if (component == null)
                {
                    continue;
                }

                if (component.IsDisposed)
                {
                    continue;
                }

                var iUpdateSystems = typeSystems.GetSystems(component.GetType(), typeof(IUpdateSystem));
                if (iUpdateSystems == null)
                {
                    continue;
                }

                queue.Enqueue(instanceId);

                foreach (IUpdateSystem iUpdateSystem in iUpdateSystems)
                {
                    try
                    {
                        iUpdateSystem.Run(component);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }
        }

        public void LateUpdate()
        {
            var queue = queues[(int)InstanceQueueIndex.LateUpdate];
            var count = queue.Count;
            while (count-- > 0)
            {
                var instanceId = queue.Dequeue();
                var component = Root.Instance.Get(instanceId);
                if (component == null)
                {
                    continue;
                }

                if (component.IsDisposed)
                {
                    continue;
                }

                var iLateUpdateSystems = typeSystems.GetSystems(component.GetType(), typeof(ILateUpdateSystem));
                if (iLateUpdateSystems == null)
                {
                    continue;
                }

                queue.Enqueue(instanceId);

                foreach (ILateUpdateSystem iLateUpdateSystem in iLateUpdateSystems)
                {
                    try
                    {
                        iLateUpdateSystem.Run(component);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }
        }

        public async ETTask PublishAsync<T>(Scene scene, T a) where T : struct
        {
            List<EventInfo> iEvents;
            if (!allEvents.TryGetValue(typeof(T), out iEvents))
            {
                return;
            }

            using var list = ListComponent<ETTask>.Create();

            foreach (var eventInfo in iEvents)
            {
                if (scene.SceneType != eventInfo.SceneType && eventInfo.SceneType != SceneType.None)
                {
                    continue;
                }

                if (!(eventInfo.IEvent is AEvent<T> aEvent))
                {
                    Log.Error($"event error: {eventInfo.IEvent.GetType().Name}");
                    continue;
                }

                list.Add(aEvent.Handle(scene, a));
            }

            try
            {
                await ETTaskHelper.WaitAll(list);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public void Publish<T>(Scene scene, T a) where T : struct
        {
            List<EventInfo> iEvents;
            if (!allEvents.TryGetValue(typeof(T), out iEvents))
            {
                return;
            }

            var sceneType = scene.SceneType;
            foreach (var eventInfo in iEvents)
            {
                if (sceneType != eventInfo.SceneType && eventInfo.SceneType != SceneType.None)
                {
                    continue;
                }


                if (!(eventInfo.IEvent is AEvent<T> aEvent))
                {
                    Log.Error($"event error: {eventInfo.IEvent.GetType().Name}");
                    continue;
                }

                aEvent.Handle(scene, a).Coroutine();
            }
        }

        // Invoke跟Publish的区别(特别注意)
        // Invoke类似函数，必须有被调用方，否则异常，调用者跟被调用者属于同一模块，比如MoveComponent中的Timer计时器，调用跟被调用的代码均属于移动模块
        // 既然Invoke跟函数一样，那么为什么不使用函数呢? 因为有时候不方便直接调用，比如Config加载，在客户端跟服务端加载方式不一样。比如TimerComponent需要根据Id分发
        // 注意，不要把Invoke当函数使用，这样会造成代码可读性降低，能用函数不要用Invoke
        // publish是事件，抛出去可以没人订阅，调用者跟被调用者属于两个模块，比如任务系统需要知道道具使用的信息，则订阅道具使用事件
        public void Invoke<A>(int type, A args) where A : struct
        {
            if (!allInvokes.TryGetValue(typeof(A), out var invokeHandlers))
            {
                throw new Exception($"Invoke error: {typeof(A).Name}");
            }

            if (!invokeHandlers.TryGetValue(type, out var invokeHandler))
            {
                throw new Exception($"Invoke error: {typeof(A).Name} {type}");
            }

            var aInvokeHandler = invokeHandler as AInvokeHandler<A>;
            if (aInvokeHandler == null)
            {
                throw new Exception($"Invoke error, not AInvokeHandler: {typeof(A).Name} {type}");
            }

            aInvokeHandler.Handle(args);
        }

        public T Invoke<A, T>(int type, A args) where A : struct
        {
            if (!allInvokes.TryGetValue(typeof(A), out var invokeHandlers))
            {
                throw new Exception($"Invoke error: {typeof(A).Name}");
            }

            if (!invokeHandlers.TryGetValue(type, out var invokeHandler))
            {
                throw new Exception($"Invoke error: {typeof(A).Name} {type}");
            }

            var aInvokeHandler = invokeHandler as AInvokeHandler<A, T>;
            if (aInvokeHandler == null)
            {
                throw new Exception($"Invoke error, not AInvokeHandler: {typeof(T).Name} {type}");
            }

            return aInvokeHandler.Handle(args);
        }

        public void Invoke<A>(A args) where A : struct
        {
            Invoke(0, args);
        }

        public T Invoke<A, T>(A args) where A : struct
        {
            return Invoke<A, T>(0, args);
        }
    }
}