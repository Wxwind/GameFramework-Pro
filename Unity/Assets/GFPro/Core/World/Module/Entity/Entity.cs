using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace GFPro
{
    [Flags]
    public enum EntityStatus : byte
    {
        None        = 0,
        IsFromPool  = 1,
        IsRegister  = 1 << 1,
        IsComponent = 1 << 2,
        IsCreated   = 1 << 3,
        IsNew       = 1 << 4
    }

    public partial class Entity : DisposeObject
    {
#if ENABLE_VIEW && UNITY_EDITOR
        private UnityEngine.GameObject viewGO;
#endif

        [BsonIgnore] public long InstanceId { get; protected set; }

        protected Entity()
        {
        }

        [BsonIgnore] private EntityStatus status = EntityStatus.None;

        [BsonIgnore]
        private bool IsFromPool
        {
            get => (status & EntityStatus.IsFromPool) == EntityStatus.IsFromPool;
            set
            {
                if (value)
                {
                    status |= EntityStatus.IsFromPool;
                }
                else
                {
                    status &= ~EntityStatus.IsFromPool;
                }
            }
        }

        [BsonIgnore]
        protected bool IsRegister
        {
            get => (status & EntityStatus.IsRegister) == EntityStatus.IsRegister;
            set
            {
                if (IsRegister == value)
                {
                    return;
                }

                if (value)
                {
                    status |= EntityStatus.IsRegister;
                }
                else
                {
                    status &= ~EntityStatus.IsRegister;
                }


                if (!value)
                {
                    Root.Instance.Remove(InstanceId);
                }
                else
                {
                    Root.Instance.Add(this);
                    EventSystem.Instance.RegisterSystem(this);
                }

#if ENABLE_VIEW && UNITY_EDITOR
                if (value)
                {
                    this.viewGO = new UnityEngine.GameObject(this.ViewName);
                    this.viewGO.AddComponent<ComponentView>().Component = this;
                    this.viewGO.transform.SetParent(this.Parent == null? 
                            UnityEngine.GameObject.Find("Global").transform : this.Parent.viewGO.transform);
                }
                else
                {
                    UnityEngine.Object.Destroy(this.viewGO);
                }
#endif
            }
        }

        protected virtual string ViewName => GetType().Name;

        [BsonIgnore]
        private bool IsComponent
        {
            get => (status & EntityStatus.IsComponent) == EntityStatus.IsComponent;
            set
            {
                if (value)
                {
                    status |= EntityStatus.IsComponent;
                }
                else
                {
                    status &= ~EntityStatus.IsComponent;
                }
            }
        }

        [BsonIgnore]
        protected bool IsCreated
        {
            get => (status & EntityStatus.IsCreated) == EntityStatus.IsCreated;
            set
            {
                if (value)
                {
                    status |= EntityStatus.IsCreated;
                }
                else
                {
                    status &= ~EntityStatus.IsCreated;
                }
            }
        }

        [BsonIgnore]
        protected bool IsNew
        {
            get => (status & EntityStatus.IsNew) == EntityStatus.IsNew;
            set
            {
                if (value)
                {
                    status |= EntityStatus.IsNew;
                }
                else
                {
                    status &= ~EntityStatus.IsNew;
                }
            }
        }

        [BsonIgnore] public bool IsDisposed => InstanceId == 0;

        [BsonIgnore] protected Entity parent;

        // 可以改变parent，但是不能设置为null
        [BsonIgnore]
        public Entity Parent
        {
            get => parent;
            private set
            {
                if (value == null)
                {
                    throw new Exception($"cant set parent null: {GetType().Name}");
                }

                if (value == this)
                {
                    throw new Exception($"cant set parent self: {GetType().Name}");
                }

                // 严格限制parent必须要有domain,也就是说parent必须在数据树上面
                if (value.Domain == null)
                {
                    throw new Exception($"cant set parent because parent domain is null: {GetType().Name} {value.GetType().Name}");
                }

                if (parent != null) // 之前有parent
                {
                    // parent相同，不设置
                    if (parent == value)
                    {
                        Log.Error($"重复设置了Parent: {GetType().Name} parent: {parent.GetType().Name}");
                        return;
                    }

                    parent.RemoveFromChildren(this);
                }

                parent = value;
                IsComponent = false;
                parent.AddToChildren(this);
                Domain = parent.domain;

#if ENABLE_VIEW && UNITY_EDITOR
                this.viewGO.GetComponent<ComponentView>().Component = this;
                this.viewGO.transform.SetParent(this.Parent == null ?
                        UnityEngine.GameObject.Find("Global").transform : this.Parent.viewGO.transform);
                foreach (var child in this.Children.Values)
                {
                    child.viewGO.transform.SetParent(this.viewGO.transform);
                }
                foreach (var comp in this.Components.Values)
                {
                    comp.viewGO.transform.SetParent(this.viewGO.transform);
                }
#endif
            }
        }

        // 该方法只能在AddComponent中调用，其他人不允许调用
        [BsonIgnore]
        private Entity ComponentParent
        {
            set
            {
                if (value == null)
                {
                    throw new Exception($"cant set parent null: {GetType().Name}");
                }

                if (value == this)
                {
                    throw new Exception($"cant set parent self: {GetType().Name}");
                }

                // 严格限制parent必须要有domain,也就是说parent必须在数据树上面
                if (value.Domain == null)
                {
                    throw new Exception($"cant set parent because parent domain is null: {GetType().Name} {value.GetType().Name}");
                }

                if (parent != null) // 之前有parent
                {
                    // parent相同，不设置
                    if (parent == value)
                    {
                        Log.Error($"重复设置了Parent: {GetType().Name} parent: {parent.GetType().Name}");
                        return;
                    }

                    parent.RemoveFromComponents(this);
                }

                parent = value;
                IsComponent = true;
                parent.AddToComponents(this);
                Domain = parent.domain;
            }
        }

        public T GetParent<T>() where T : Entity
        {
            return Parent as T;
        }

        [BsonIgnoreIfDefault]
        [BsonDefaultValue(0L)]
        [BsonElement]
        [BsonId]
        public long Id { get; set; }

        [BsonIgnore] protected Entity domain;

        [BsonIgnore]
        public Entity Domain
        {
            get => domain;
            private set
            {
                if (value == null)
                {
                    throw new Exception($"domain cant set null: {GetType().Name}");
                }

                if (domain == value)
                {
                    return;
                }

                var preDomain = domain;
                domain = value;

                if (preDomain == null)
                {
                    InstanceId = IdGenerater.Instance.GenerateInstanceId();
                    IsRegister = true;

                    // 反序列化出来的需要设置父子关系
                    if (componentsDB != null)
                    {
                        foreach (var component in componentsDB)
                        {
                            component.IsComponent = true;
                            Components.Add(component.GetType(), component);
                            component.parent = this;
                        }
                    }

                    if (childrenDB != null)
                    {
                        foreach (var child in childrenDB)
                        {
                            child.IsComponent = false;
                            Children.Add(child.Id, child);
                            child.parent = this;
                        }
                    }
                }

                // 递归设置孩子的Domain
                if (children != null)
                {
                    foreach (var entity in children.Values)
                    {
                        entity.Domain = domain;
                    }
                }

                if (components != null)
                {
                    foreach (var component in components.Values)
                    {
                        component.Domain = domain;
                    }
                }

                if (!IsCreated)
                {
                    IsCreated = true;
                    EventSystem.Instance.Deserialize(this);
                }
            }
        }

        [BsonElement("Children")] [BsonIgnoreIfNull]
        private HashSet<Entity> childrenDB;

        [BsonIgnore] private Dictionary<long, Entity> children;

        [BsonIgnore]
        public Dictionary<long, Entity> Children
        {
            get { return children ??= ObjectPool.Instance.Fetch<Dictionary<long, Entity>>(); }
        }

        private void AddToChildren(Entity entity)
        {
            Children.Add(entity.Id, entity);
            AddToChildrenDB(entity);
        }

        private void RemoveFromChildren(Entity entity)
        {
            if (children == null)
            {
                return;
            }

            children.Remove(entity.Id);

            if (children.Count == 0)
            {
                ObjectPool.Instance.Recycle(children);
                children = null;
            }

            RemoveFromChildrenDB(entity);
        }

        private void AddToChildrenDB(Entity entity)
        {
            if (!(entity is ISerializeToEntity))
            {
                return;
            }

            childrenDB ??= ObjectPool.Instance.Fetch<HashSet<Entity>>();

            childrenDB.Add(entity);
        }

        private void RemoveFromChildrenDB(Entity entity)
        {
            if (!(entity is ISerializeToEntity))
            {
                return;
            }

            if (childrenDB == null)
            {
                return;
            }

            childrenDB.Remove(entity);

            if (childrenDB.Count == 0 && IsNew)
            {
                ObjectPool.Instance.Recycle(childrenDB);
                childrenDB = null;
            }
        }

        [BsonElement("C")] [BsonIgnoreIfNull] private HashSet<Entity> componentsDB;

        [BsonIgnore] private Dictionary<Type, Entity> components;

        [BsonIgnore]
        public Dictionary<Type, Entity> Components
        {
            get { return components ??= ObjectPool.Instance.Fetch<Dictionary<Type, Entity>>(); }
        }

        public override void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsRegister = false;
            InstanceId = 0;

            // 清理Children
            if (children != null)
            {
                foreach (var child in children.Values)
                {
                    child.Dispose();
                }

                children.Clear();
                ObjectPool.Instance.Recycle(children);
                children = null;

                if (childrenDB != null)
                {
                    childrenDB.Clear();
                    // 创建的才需要回到池中,从db中不需要回收
                    if (IsNew)
                    {
                        ObjectPool.Instance.Recycle(childrenDB);
                        childrenDB = null;
                    }
                }
            }

            // 清理Component
            if (components != null)
            {
                foreach (var kv in components)
                {
                    kv.Value.Dispose();
                }

                components.Clear();
                ObjectPool.Instance.Recycle(components);
                components = null;

                // 创建的才需要回到池中,从db中不需要回收
                if (componentsDB != null)
                {
                    componentsDB.Clear();
                    if (IsNew)
                    {
                        ObjectPool.Instance.Recycle(componentsDB);
                        componentsDB = null;
                    }
                }
            }

            // 触发Destroy事件
            if (this is IDestroy)
            {
                EventSystem.Instance.Destroy(this);
            }

            domain = null;

            if (parent != null && !parent.IsDisposed)
            {
                if (IsComponent)
                {
                    parent.RemoveComponent(this);
                }
                else
                {
                    parent.RemoveFromChildren(this);
                }
            }

            parent = null;

            base.Dispose();

            if (IsFromPool)
            {
                ObjectPool.Instance.Recycle(this);
            }

            status = EntityStatus.None;
        }

        private void AddToComponentsDB(Entity component)
        {
            if (!(component is ISerializeToEntity))
            {
                return;
            }

            componentsDB ??= ObjectPool.Instance.Fetch<HashSet<Entity>>();
            componentsDB.Add(component);
        }

        private void RemoveFromComponentsDB(Entity component)
        {
            if (!(component is ISerializeToEntity))
            {
                return;
            }

            if (componentsDB == null)
            {
                return;
            }

            componentsDB.Remove(component);
            if (componentsDB.Count == 0 && IsNew)
            {
                ObjectPool.Instance.Recycle(componentsDB);
                componentsDB = null;
            }
        }

        private void AddToComponents(Entity component)
        {
            Components.Add(component.GetType(), component);
            AddToComponentsDB(component);
        }

        private void RemoveFromComponents(Entity component)
        {
            if (components == null)
            {
                return;
            }

            components.Remove(component.GetType());

            if (components.Count == 0)
            {
                ObjectPool.Instance.Recycle(components);
                components = null;
            }

            RemoveFromComponentsDB(component);
        }

        public K GetChild<K>(long id) where K : Entity
        {
            if (children == null)
            {
                return null;
            }

            children.TryGetValue(id, out var child);
            return child as K;
        }

        public void RemoveChild(long id)
        {
            if (children == null)
            {
                return;
            }

            if (!children.TryGetValue(id, out var child))
            {
                return;
            }

            children.Remove(id);
            child.Dispose();
        }

        public void RemoveComponent<K>() where K : Entity
        {
            if (IsDisposed)
            {
                return;
            }

            if (components == null)
            {
                return;
            }

            var type = typeof(K);
            var c = GetComponent(type);
            if (c == null)
            {
                return;
            }

            RemoveFromComponents(c);
            c.Dispose();
        }

        public void RemoveComponent(Entity component)
        {
            if (IsDisposed)
            {
                return;
            }

            if (components == null)
            {
                return;
            }

            var c = GetComponent(component.GetType());
            if (c == null)
            {
                return;
            }

            if (c.InstanceId != component.InstanceId)
            {
                return;
            }

            RemoveFromComponents(c);
            c.Dispose();
        }

        public void RemoveComponent(Type type)
        {
            if (IsDisposed)
            {
                return;
            }

            var c = GetComponent(type);
            if (c == null)
            {
                return;
            }

            RemoveFromComponents(c);
            c.Dispose();
        }

        public K GetComponent<K>() where K : Entity
        {
            if (components == null)
            {
                return null;
            }

            Entity component;
            if (!components.TryGetValue(typeof(K), out component))
            {
                return default;
            }

            // 如果有IGetComponent接口，则触发GetComponentSystem
            if (this is IGetComponent)
            {
                EventSystem.Instance.GetComponent(this, component);
            }

            return (K)component;
        }

        public Entity GetComponent(Type type)
        {
            if (components == null)
            {
                return null;
            }

            Entity component;
            if (!components.TryGetValue(type, out component))
            {
                return null;
            }

            // 如果有IGetComponent接口，则触发GetComponentSystem
            if (this is IGetComponent)
            {
                EventSystem.Instance.GetComponent(this, component);
            }

            return component;
        }

        private static Entity Create(Type type, bool isFromPool)
        {
            Entity component;
            if (isFromPool)
            {
                component = (Entity)ObjectPool.Instance.Fetch(type);
            }
            else
            {
                component = Activator.CreateInstance(type) as Entity;
            }

            component.IsFromPool = isFromPool;
            component.IsCreated = true;
            component.IsNew = true;
            component.Id = 0;
            return component;
        }

        public Entity AddComponent(Entity component)
        {
            var type = component.GetType();
            if (components != null && components.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            component.ComponentParent = this;

            if (this is IAddComponent)
            {
                EventSystem.Instance.AddComponent(this, component);
            }

            return component;
        }

        public Entity AddComponent(Type type, bool isFromPool = false)
        {
            if (components != null && components.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            var component = Create(type, isFromPool);
            component.Id = Id;
            component.ComponentParent = this;
            EventSystem.Instance.Awake(component);

            if (this is IAddComponent)
            {
                EventSystem.Instance.AddComponent(this, component);
            }

            return component;
        }

        public K AddComponent<K>(bool isFromPool = false) where K : Entity, IAwake, new()
        {
            var type = typeof(K);
            if (components != null && components.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            var component = Create(type, isFromPool);
            component.Id = Id;
            component.ComponentParent = this;
            EventSystem.Instance.Awake(component);

            if (this is IAddComponent)
            {
                EventSystem.Instance.AddComponent(this, component);
            }

            return component as K;
        }

        public K AddComponent<K, P1>(P1 p1, bool isFromPool = false) where K : Entity, IAwake<P1>, new()
        {
            var type = typeof(K);
            if (components != null && components.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            var component = Create(type, isFromPool);
            component.Id = Id;
            component.ComponentParent = this;
            EventSystem.Instance.Awake(component, p1);

            if (this is IAddComponent)
            {
                EventSystem.Instance.AddComponent(this, component);
            }

            return component as K;
        }

        public K AddComponent<K, P1, P2>(P1 p1, P2 p2, bool isFromPool = false) where K : Entity, IAwake<P1, P2>, new()
        {
            var type = typeof(K);
            if (components != null && components.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            var component = Create(type, isFromPool);
            component.Id = Id;
            component.ComponentParent = this;
            EventSystem.Instance.Awake(component, p1, p2);

            if (this is IAddComponent)
            {
                EventSystem.Instance.AddComponent(this, component);
            }

            return component as K;
        }

        public K AddComponent<K, P1, P2, P3>(P1 p1, P2 p2, P3 p3, bool isFromPool = false) where K : Entity, IAwake<P1, P2, P3>, new()
        {
            var type = typeof(K);
            if (components != null && components.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            var component = Create(type, isFromPool);
            component.Id = Id;
            component.ComponentParent = this;
            EventSystem.Instance.Awake(component, p1, p2, p3);

            if (this is IAddComponent)
            {
                EventSystem.Instance.AddComponent(this, component);
            }

            return component as K;
        }

        public Entity AddChild(Entity entity)
        {
            entity.Parent = this;
            return entity;
        }

        public T AddChild<T>(bool isFromPool = false) where T : Entity, IAwake
        {
            var type = typeof(T);
            var component = (T)Create(type, isFromPool);
            component.Id = IdGenerater.Instance.GenerateId();
            component.Parent = this;

            EventSystem.Instance.Awake(component);
            return component;
        }

        public T AddChild<T, A>(A a, bool isFromPool = false) where T : Entity, IAwake<A>
        {
            var type = typeof(T);
            var component = (T)Create(type, isFromPool);
            component.Id = IdGenerater.Instance.GenerateId();
            component.Parent = this;

            EventSystem.Instance.Awake(component, a);
            return component;
        }

        public T AddChild<T, A, B>(A a, B b, bool isFromPool = false) where T : Entity, IAwake<A, B>
        {
            var type = typeof(T);
            var component = (T)Create(type, isFromPool);
            component.Id = IdGenerater.Instance.GenerateId();
            component.Parent = this;

            EventSystem.Instance.Awake(component, a, b);
            return component;
        }

        public T AddChild<T, A, B, C>(A a, B b, C c, bool isFromPool = false) where T : Entity, IAwake<A, B, C>
        {
            var type = typeof(T);
            var component = (T)Create(type, isFromPool);
            component.Id = IdGenerater.Instance.GenerateId();
            component.Parent = this;

            EventSystem.Instance.Awake(component, a, b, c);
            return component;
        }

        public T AddChild<T, A, B, C, D>(A a, B b, C c, D d, bool isFromPool = false) where T : Entity, IAwake<A, B, C, D>
        {
            var type = typeof(T);
            var component = (T)Create(type, isFromPool);
            component.Id = IdGenerater.Instance.GenerateId();
            component.Parent = this;

            EventSystem.Instance.Awake(component, a, b, c, d);
            return component;
        }

        public T AddChildWithId<T>(long id, bool isFromPool = false) where T : Entity, IAwake, new()
        {
            var type = typeof(T);
            var component = Create(type, isFromPool) as T;
            component.Id = id;
            component.Parent = this;
            EventSystem.Instance.Awake(component);
            return component;
        }

        public T AddChildWithId<T, A>(long id, A a, bool isFromPool = false) where T : Entity, IAwake<A>
        {
            var type = typeof(T);
            var component = (T)Create(type, isFromPool);
            component.Id = id;
            component.Parent = this;

            EventSystem.Instance.Awake(component, a);
            return component;
        }

        public T AddChildWithId<T, A, B>(long id, A a, B b, bool isFromPool = false) where T : Entity, IAwake<A, B>
        {
            var type = typeof(T);
            var component = (T)Create(type, isFromPool);
            component.Id = id;
            component.Parent = this;

            EventSystem.Instance.Awake(component, a, b);
            return component;
        }

        public T AddChildWithId<T, A, B, C>(long id, A a, B b, C c, bool isFromPool = false) where T : Entity, IAwake<A, B, C>
        {
            var type = typeof(T);
            var component = (T)Create(type, isFromPool);
            component.Id = id;
            component.Parent = this;

            EventSystem.Instance.Awake(component, a, b, c);
            return component;
        }
    }
}