﻿using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GFPro.ObjectPool;
using GFPro.Resource;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GFPro.Entity
{
    /// <summary>
    /// 实体组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Entity")]
    public sealed partial class EntityComponent : GameFrameworkComponent
    {
        private const int DefaultPriority = 0;

        private readonly List<IEntity> m_InternalEntityResults = new();

        [SerializeField] private Transform m_InstanceRoot;

        [SerializeField] private EntityGroupConfig[] m_EntityGroups;

        private readonly Dictionary<int, EntityInfo>     m_EntityInfoDict          = new();
        private readonly Dictionary<string, EntityGroup> m_EntityGroupsDict        = new();
        private readonly Dictionary<int, int>            m_EntitiesBeingLoaded     = new();
        private readonly HashSet<int>                    m_EntitiesToReleaseOnLoad = new();
        private readonly Queue<EntityInfo>               m_RecycleQueue            = new();
        private          IObjectPoolComponent            m_ObjectPoolComponent;
        private          IResourceComponent              m_ResourceComponent;
        private          IEntityHelper                   m_EntityHelper;
        private          int                             m_Serial;
        private          bool                            m_IsShutdown;


        /// <summary>
        /// 获取实体数量。
        /// </summary>
        public int EntityCount => m_EntityInfoDict.Count;

        /// <summary>
        /// 获取实体组数量。
        /// </summary>
        public int EntityGroupCount => m_EntityGroupsDict.Count;


        private void Start()
        {
            m_ResourceComponent = GameEntry.GetComponent<ResourceComponent>();
            m_ObjectPoolComponent = GameEntry.GetComponent<ObjectPoolComponent>();

            m_EntityHelper = new DefaultEntityHelper();

            if (m_InstanceRoot == null)
            {
                m_InstanceRoot = new GameObject("Entity Instances").transform;
                m_InstanceRoot.SetParent(gameObject.transform);
                m_InstanceRoot.localScale = Vector3.one;
            }

            for (var i = 0; i < m_EntityGroups.Length; i++)
            {
                if (!AddEntityGroup(m_EntityGroups[i].Name, m_EntityGroups[i].InstanceAutoReleaseInterval,
                        m_EntityGroups[i].InstanceCapacity, m_EntityGroups[i].InstanceExpireTime,
                        m_EntityGroups[i].InstancePriority))
                {
                    Log.Warning("Add entity group '{0}' failure.", m_EntityGroups[i].Name);
                }
            }
        }

        private void Update()
        {
            while (m_RecycleQueue.Count > 0)
            {
                var entityInfo = m_RecycleQueue.Dequeue();
                var entity = entityInfo.Entity;
                var entityGroup = (EntityGroup)entity.EntityGroup;
                if (entityGroup == null)
                {
                    throw new GameFrameworkException("Entity group is invalid.");
                }

                entityInfo.Status = EntityStatus.WillRecycle;
                entity.OnRecycle();
                entityInfo.Status = EntityStatus.Recycled;
                entityGroup.UnspawnEntity(entity);
                ReferencePool.Release(entityInfo);
            }

            foreach (var entityGroup in m_EntityGroupsDict)
            {
                entityGroup.Value.Update(Time.deltaTime, Time.unscaledDeltaTime);
            }
        }

        private void OnDestroy()
        {
            m_IsShutdown = true;
            HideAllLoadedEntities();
            m_EntityGroupsDict.Clear();
            m_EntitiesBeingLoaded.Clear();
            m_EntitiesToReleaseOnLoad.Clear();
            m_RecycleQueue.Clear();
        }

        /// <summary>
        /// 是否存在实体组。
        /// </summary>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <returns>是否存在实体组。</returns>
        public bool HasEntityGroup(string entityGroupName)
        {
            return m_EntityGroupsDict.ContainsKey(entityGroupName);
        }

        /// <summary>
        /// 获取实体组。
        /// </summary>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <returns>要获取的实体组。</returns>
        public IEntityGroup GetEntityGroup(string entityGroupName)
        {
            if (string.IsNullOrEmpty(entityGroupName))
            {
                throw new GameFrameworkException("Entity group name is invalid.");
            }

            if (m_EntityGroupsDict.TryGetValue(entityGroupName, out var entityGroupConfig))
            {
                return entityGroupConfig;
            }

            return null;
        }

        /// <summary>
        /// 获取所有实体组。
        /// </summary>
        /// <returns>所有实体组。</returns>
        public IEntityGroup[] GetAllEntityGroups()
        {
            var index = 0;
            var results = new IEntityGroup[m_EntityGroupsDict.Count];
            foreach (var entityGroup in m_EntityGroupsDict)
            {
                results[index++] = entityGroup.Value;
            }

            return results;
        }

        /// <summary>
        /// 获取所有实体组。
        /// </summary>
        /// <param name="results">所有实体组。</param>
        public void GetAllEntityGroups(List<IEntityGroup> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (var entityGroup in m_EntityGroupsDict)
            {
                results.Add(entityGroup.Value);
            }
        }

        /// <summary>
        /// 增加实体组。
        /// </summary>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <param name="instanceAutoReleaseInterval">实体实例对象池自动释放可释放对象的间隔秒数。</param>
        /// <param name="instanceCapacity">实体实例对象池容量。</param>
        /// <param name="instanceExpireTime">实体实例对象池对象过期秒数。</param>
        /// <param name="instancePriority">实体实例对象池的优先级。</param>
        /// <returns>是否增加实体组成功。</returns>
        public bool AddEntityGroup(string entityGroupName, float instanceAutoReleaseInterval, int instanceCapacity,
            float instanceExpireTime, int instancePriority)
        {
            if (HasEntityGroup(entityGroupName))
            {
                return false;
            }

            if (string.IsNullOrEmpty(entityGroupName))
            {
                throw new GameFrameworkException("Entity group name is invalid.");
            }

            if (m_ObjectPoolComponent == null)
            {
                throw new GameFrameworkException("You must set object pool manager first.");
            }

            if (HasEntityGroup(entityGroupName))
            {
                return false;
            }

            var entityGroupHelper = new GameObject();

            entityGroupHelper.name = Utility.Text.Format("Entity Group - {0}", entityGroupName);
            var transform = entityGroupHelper.transform;
            transform.SetParent(m_InstanceRoot);
            transform.localScale = Vector3.one;

            m_EntityGroupsDict.Add(entityGroupName,
                new EntityGroup(entityGroupName, instanceAutoReleaseInterval, instanceCapacity, instanceExpireTime,
                    instancePriority, m_ObjectPoolComponent, entityGroupHelper));

            return true;
        }

        /// <summary>
        /// 是否存在实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <returns>是否存在实体。</returns>
        public bool HasEntity(int entityId)
        {
            return m_EntityInfoDict.ContainsKey(entityId);
        }

        /// <summary>
        /// 是否存在实体。
        /// </summary>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <returns>是否存在实体。</returns>
        public bool HasEntity(string entityAssetName)
        {
            if (string.IsNullOrEmpty(entityAssetName))
            {
                throw new GameFrameworkException("Entity asset name is invalid.");
            }

            foreach (var entityInfo in m_EntityInfoDict)
            {
                if (entityInfo.Value.Entity.EntityAssetName == entityAssetName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <returns>实体。</returns>
        public Entity GetEntity(int entityId)
        {
            var entityInfo = GetEntityInfo(entityId);
            if (entityInfo == null)
            {
                return null;
            }

            return entityInfo.Entity as Entity;
        }

        /// <summary>
        /// 获取实体。
        /// </summary>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <returns>要获取的实体。</returns>
        public Entity GetEntity(string entityAssetName)
        {
            if (string.IsNullOrEmpty(entityAssetName))
            {
                throw new GameFrameworkException("Entity asset name is invalid.");
            }

            foreach (var entityInfo in m_EntityInfoDict)
            {
                if (entityInfo.Value.Entity.EntityAssetName == entityAssetName)
                {
                    return entityInfo.Value.Entity as Entity;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取实体。
        /// </summary>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <returns>要获取的实体。</returns>
        public Entity[] GetEntities(string entityAssetName)
        {
            if (string.IsNullOrEmpty(entityAssetName))
            {
                throw new GameFrameworkException("Entity asset name is invalid.");
            }

            var results = new List<IEntity>();
            foreach (var entityInfo in m_EntityInfoDict)
            {
                if (entityInfo.Value.Entity.EntityAssetName == entityAssetName)
                {
                    results.Add(entityInfo.Value.Entity);
                }
            }

            var entities = results.ToArray();

            var entityImpls = new Entity[entities.Length];
            for (var i = 0; i < entities.Length; i++)
            {
                entityImpls[i] = (Entity)entities[i];
            }

            return entityImpls;
        }

        /// <summary>
        /// 获取实体。
        /// </summary>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <param name="results">要获取的实体。</param>
        public void GetEntities(string entityAssetName, List<Entity> results)
        {
            if (results == null)
            {
                Log.Error("Results is invalid.");
                return;
            }

            results.Clear();

            if (string.IsNullOrEmpty(entityAssetName))
            {
                throw new GameFrameworkException("Entity asset name is invalid.");
            }

            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (var entityInfo in m_EntityInfoDict)
            {
                if (entityInfo.Value.Entity.EntityAssetName == entityAssetName)
                {
                    results.Add(entityInfo.Value.Entity as Entity);
                }
            }
        }

        /// <summary>
        /// 获取所有已加载的实体。
        /// </summary>
        /// <returns>所有已加载的实体。</returns>
        public Entity[] GetAllLoadedEntities()
        {
            var index = 0;
            var results = new Entity[m_EntityInfoDict.Count];
            foreach (var entityInfo in m_EntityInfoDict)
            {
                results[index++] = entityInfo.Value.Entity as Entity;
            }

            return results;
        }

        /// <summary>
        /// 获取所有已加载的实体。
        /// </summary>
        /// <param name="results">所有已加载的实体。</param>
        public void GetAllLoadedEntities(List<Entity> results)
        {
            if (results == null)
            {
                Log.Error("Results is invalid.");
                return;
            }

            results.Clear();
            foreach (var entityInfo in m_EntityInfoDict)
            {
                results.Add(entityInfo.Value.Entity as Entity);
            }
        }

        /// <summary>
        /// 获取所有正在加载实体的编号。
        /// </summary>
        /// <returns>所有正在加载实体的编号。</returns>
        public int[] GetAllLoadingEntityIds()
        {
            var index = 0;
            var results = new int[m_EntitiesBeingLoaded.Count];
            foreach (var entityBeingLoaded in m_EntitiesBeingLoaded)
            {
                results[index++] = entityBeingLoaded.Key;
            }

            return results;
        }

        /// <summary>
        /// 获取所有正在加载实体的编号。
        /// </summary>
        /// <param name="results">所有正在加载实体的编号。</param>
        public void GetAllLoadingEntityIds(List<int> results)
        {
            if (results == null)
            {
                Log.Error("Results is invalid.");
                return;
            }

            results.Clear();
            foreach (var entityBeingLoaded in m_EntitiesBeingLoaded)
            {
                results.Add(entityBeingLoaded.Key);
            }
        }

        /// <summary>
        /// 是否正在加载实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <returns>是否正在加载实体。</returns>
        public bool IsLoadingEntity(int entityId)
        {
            return m_EntitiesBeingLoaded.ContainsKey(entityId);
        }

        /// <summary>
        /// 是否是合法的实体。
        /// </summary>
        /// <param name="entity">实体。</param>
        /// <returns>实体是否合法。</returns>
        public bool IsValidEntity(Entity entity)
        {
            if (entity == null)
            {
                return false;
            }

            return HasEntity(entity.Id);
        }

        /// <summary>
        /// <summary>
        /// 显示实体。
        /// </summary>
        /// <typeparam name="T">实体逻辑类型。</typeparam>
        /// <param name="entityId">实体编号。</param>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        public UniTask<IEntity> ShowEntity<T>(int entityId, string entityAssetName, string entityGroupName,
            object userData = null) where T : EntityLogic
        {
            return ShowEntity(entityId, typeof(T), entityAssetName, entityGroupName, userData);
        }

        /// <summary>
        /// 显示实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <param name="entityLogicType">实体逻辑类型。</param>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        public async UniTask<IEntity> ShowEntity(int entityId, Type entityLogicType, string entityAssetName,
            string entityGroupName,
            object userData = null)
        {
            var userDataWrapper = ShowEntityInfo.Create(entityLogicType, userData);
            if (HasEntity(entityId))
            {
                throw new GameFrameworkException(Utility.Text.Format("Entity id '{0}' is already exist.", entityId));
            }

            if (IsLoadingEntity(entityId))
            {
                throw new GameFrameworkException(Utility.Text.Format("Entity '{0}' is already being loaded.",
                    entityId));
            }

            var entityGroup = (EntityGroup)GetEntityGroup(entityGroupName);
            if (entityGroup == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Entity group '{0}' is not exist.",
                    entityGroupName));
            }

            var entityInstanceObject = entityGroup.SpawnEntityInstanceObject(entityAssetName);
            if (entityInstanceObject == null)
            {
                var serialId = ++m_Serial;
                var info = InternalShowEntityInfo.Create(serialId, entityId, entityGroup, userDataWrapper);
                m_EntitiesBeingLoaded.Add(entityId, serialId);
                try
                {
                    var asset = await m_ResourceComponent.LoadAssetAsync<Object>(entityAssetName);
                    return LoadAssetSuccessCallback(entityAssetName, asset, info);
                }
                catch (Exception e)
                {
                    LoadAssetFailureCallback(entityAssetName, e.Message, info);
                    return null;
                }
            }

            var entity = InternalShowEntity(entityId, entityAssetName, entityGroup, entityInstanceObject.Target, false,
                userDataWrapper);
            return entity;
        }


        /// <summary>
        /// 隐藏实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void HideEntity(int entityId, object userData = null)
        {
            if (IsLoadingEntity(entityId))
            {
                m_EntitiesToReleaseOnLoad.Add(m_EntitiesBeingLoaded[entityId]);
                m_EntitiesBeingLoaded.Remove(entityId);
                return;
            }

            var entityInfo = GetEntityInfo(entityId);
            if (entityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find entity '{0}'.", entityId));
            }

            InternalHideEntity(entityInfo, userData);
        }


        /// <summary>
        /// 隐藏实体。
        /// </summary>
        /// <param name="entity">实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void HideEntity(Entity entity, object userData = null)
        {
            if (entity == null)
            {
                throw new GameFrameworkException("Entity is invalid.");
            }

            HideEntity(entity.Id, userData);
        }

        /// <summary>
        /// 隐藏所有已加载的实体。
        /// </summary>
        public void HideAllLoadedEntities()
        {
            HideAllLoadedEntities(null);
        }

        /// <summary>
        /// 隐藏所有已加载的实体。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        public void HideAllLoadedEntities(object userData)
        {
            while (m_EntityInfoDict.Count > 0)
            {
                foreach (var entityInfo in m_EntityInfoDict)
                {
                    InternalHideEntity(entityInfo.Value, userData);
                    break;
                }
            }
        }

        /// <summary>
        /// 隐藏所有正在加载的实体。
        /// </summary>
        public void HideAllLoadingEntities()
        {
            foreach (var entityBeingLoaded in m_EntitiesBeingLoaded)
            {
                m_EntitiesToReleaseOnLoad.Add(entityBeingLoaded.Value);
            }

            m_EntitiesBeingLoaded.Clear();
        }

        /// <summary>
        /// 获取父实体。
        /// </summary>
        /// <param name="childEntityId">要获取父实体的子实体的实体编号。</param>
        /// <returns>子实体的父实体。</returns>
        public Entity GetParentEntity(int childEntityId)
        {
            var childEntityInfo = GetEntityInfo(childEntityId);
            if (childEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find child entity '{0}'.",
                    childEntityId));
            }

            return childEntityInfo.ParentEntity as Entity;
        }

        /// <summary>
        /// 获取父实体。
        /// </summary>
        /// <param name="childEntity">要获取父实体的子实体。</param>
        /// <returns>子实体的父实体。</returns>
        public Entity GetParentEntity(Entity childEntity)
        {
            if (childEntity == null)
            {
                throw new GameFrameworkException("Child entity is invalid.");
            }

            return GetParentEntity(childEntity.Id);
        }

        /// <summary>
        /// 获取子实体数量。
        /// </summary>
        /// <param name="parentEntityId">要获取子实体数量的父实体的实体编号。</param>
        /// <returns>子实体数量。</returns>
        public int GetChildEntityCount(int parentEntityId)
        {
            var parentEntityInfo = GetEntityInfo(parentEntityId);
            if (parentEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find parent entity '{0}'.",
                    parentEntityId));
            }

            return parentEntityInfo.ChildEntityCount;
        }

        /// <summary>
        /// 获取子实体。
        /// </summary>
        /// <param name="parentEntityId">要获取子实体的父实体的实体编号。</param>
        /// <returns>子实体。</returns>
        public Entity GetChildEntity(int parentEntityId)
        {
            var parentEntityInfo = GetEntityInfo(parentEntityId);
            if (parentEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find parent entity '{0}'.",
                    parentEntityId));
            }

            return parentEntityInfo.GetChildEntity() as Entity;
        }

        /// <summary>
        /// 获取子实体。
        /// </summary>
        /// <param name="parentEntity">要获取子实体的父实体。</param>
        /// <returns>子实体。</returns>
        public Entity GetChildEntity(IEntity parentEntity)
        {
            if (parentEntity == null)
            {
                throw new GameFrameworkException("Parent entity is invalid.");
            }

            return GetChildEntity(parentEntity.Id) as Entity;
        }

        /// <summary>
        /// 获取所有子实体。
        /// </summary>
        /// <param name="parentEntityId">要获取所有子实体的父实体的实体编号。</param>
        /// <returns>所有子实体。</returns>
        public Entity[] GetChildEntities(int parentEntityId)
        {
            var parentEntityInfo = GetEntityInfo(parentEntityId);
            if (parentEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find parent entity '{0}'.",
                    parentEntityId));
            }

            var entities = parentEntityInfo.GetChildEntities();

            var entityImpls = new Entity[entities.Length];
            for (var i = 0; i < entities.Length; i++)
            {
                entityImpls[i] = (Entity)entities[i];
            }

            return entityImpls;
        }

        /// <summary>
        /// 获取所有子实体。
        /// </summary>
        /// <param name="parentEntityId">要获取所有子实体的父实体的实体编号。</param>
        /// <param name="results">所有子实体。</param>
        public void GetChildEntities(int parentEntityId, List<IEntity> results)
        {
            if (results == null)
            {
                Log.Error("Results is invalid.");
                return;
            }

            results.Clear();

            var parentEntityInfo = GetEntityInfo(parentEntityId);
            if (parentEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find parent entity '{0}'.",
                    parentEntityId));
            }

            parentEntityInfo.GetChildEntities(m_InternalEntityResults);

            foreach (var entity in m_InternalEntityResults)
            {
                results.Add((Entity)entity);
            }
        }

        /// <summary>
        /// 获取所有子实体。
        /// </summary>
        /// <param name="parentEntity">要获取所有子实体的父实体。</param>
        /// <returns>所有子实体。</returns>
        public Entity[] GetChildEntities(Entity parentEntity)
        {
            if (parentEntity == null)
            {
                throw new GameFrameworkException("Parent entity is invalid.");
            }

            return GetChildEntities(parentEntity.Id);
        }

        /// <summary>
        /// 获取所有子实体。
        /// </summary>
        /// <param name="parentEntity">要获取所有子实体的父实体。</param>
        /// <param name="results">所有子实体。</param>
        public void GetChildEntities(IEntity parentEntity, List<IEntity> results)
        {
            if (parentEntity == null)
            {
                throw new GameFrameworkException("Parent entity is invalid.");
            }

            GetChildEntities(parentEntity.Id, results);
        }


        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntityId">要附加的子实体的实体编号。</param>
        /// <param name="parentEntityId">被附加的父实体的实体编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void AttachEntity(int childEntityId, int parentEntityId, object userData = null)
        {
            AttachEntity(childEntityId, parentEntityId, string.Empty, userData);
        }


        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntityId">要附加的子实体的实体编号。</param>
        /// <param name="parentEntityId">被附加的父实体的实体编号。</param>
        /// <param name="parentTransformPath">相对于被附加父实体的位置。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void AttachEntity(int childEntityId, int parentEntityId, string parentTransformPath, object userData)
        {
            var parentEntity = GetEntity(parentEntityId);
            if (parentEntity == null)
            {
                Log.Warning("Parent entity is invalid.");
                return;
            }

            Transform parentTransform = null;
            if (string.IsNullOrEmpty(parentTransformPath))
            {
                parentTransform = parentEntity.Logic.CachedTransform;
            }
            else
            {
                parentTransform = parentEntity.Logic.CachedTransform.Find(parentTransformPath);
                if (parentTransform == null)
                {
                    Log.Warning("Can not find transform path '{0}' from parent entity '{1}'.", parentTransformPath,
                        parentEntity.Logic.Name);
                    parentTransform = parentEntity.Logic.CachedTransform;
                }
            }

            InternalAttachEntity(childEntityId, parentEntityId, AttachEntityInfo.Create(parentTransform, userData));
        }


        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntityId">要附加的子实体id。</param>
        /// <param name="parentEntityId">被附加的父实体id。</param>
        /// <param name="parentTransform">相对于被附加父实体的位置。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void AttachEntity(int childEntityId, int parentEntityId, Transform parentTransform, object userData)
        {
            var parentEntity = GetEntity(parentEntityId);
            if (parentEntity == null)
            {
                Log.Warning("Parent entity is invalid.");
                return;
            }

            if (parentTransform == null)
            {
                parentTransform = parentEntity.Logic.CachedTransform;
            }

            InternalAttachEntity(childEntityId, parentEntityId, AttachEntityInfo.Create(parentTransform, userData));
        }

        private void InternalAttachEntity(int childEntityId, int parentEntityId, object userData)
        {
            if (childEntityId == parentEntityId)
            {
                throw new GameFrameworkException(Utility.Text.Format(
                    "Can not attach entity when child entity id equals to parent entity id '{0}'.", parentEntityId));
            }

            var childEntityInfo = GetEntityInfo(childEntityId);
            if (childEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find child entity '{0}'.",
                    childEntityId));
            }

            if (childEntityInfo.Status >= EntityStatus.WillHide)
            {
                throw new GameFrameworkException(
                    Utility.Text.Format("Can not attach entity when child entity status is '{0}'.",
                        childEntityInfo.Status));
            }

            var parentEntityInfo = GetEntityInfo(parentEntityId);
            if (parentEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find parent entity '{0}'.",
                    parentEntityId));
            }

            if (parentEntityInfo.Status >= EntityStatus.WillHide)
            {
                throw new GameFrameworkException(Utility.Text.Format(
                    "Can not attach entity when parent entity status is '{0}'.", parentEntityInfo.Status));
            }

            var childEntity = childEntityInfo.Entity;
            var parentEntity = parentEntityInfo.Entity;
            DetachEntity(childEntity.Id, userData);
            childEntityInfo.ParentEntity = parentEntity;
            parentEntityInfo.AddChildEntity(childEntity);
            parentEntity.OnAttached(childEntity, userData);
            childEntity.OnAttachTo(parentEntity, userData);
        }


        /// <summary>
        /// 解除子实体。
        /// </summary>
        /// <param name="childEntityId">要解除的子实体的实体编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void DetachEntity(int childEntityId, object userData = null)
        {
            var childEntityInfo = GetEntityInfo(childEntityId);
            if (childEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find child entity '{0}'.",
                    childEntityId));
            }

            var parentEntity = childEntityInfo.ParentEntity;
            if (parentEntity == null)
            {
                return;
            }

            var parentEntityInfo = GetEntityInfo(parentEntity.Id);
            if (parentEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find parent entity '{0}'.",
                    parentEntity.Id));
            }

            var childEntity = childEntityInfo.Entity;
            childEntityInfo.ParentEntity = null;
            parentEntityInfo.RemoveChildEntity(childEntity);
            parentEntity.OnDetached(childEntity, userData);
            childEntity.OnDetachFrom(parentEntity, userData);
        }


        /// <summary>
        /// 解除所有子实体。
        /// </summary>
        /// <param name="parentEntityId">被解除的父实体的实体编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void DetachChildEntities(int parentEntityId, object userData = null)
        {
            var parentEntityInfo = GetEntityInfo(parentEntityId);
            if (parentEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find parent entity '{0}'.",
                    parentEntityId));
            }

            while (parentEntityInfo.ChildEntityCount > 0)
            {
                var childEntity = parentEntityInfo.GetChildEntity();
                DetachEntity(childEntity.Id, userData);
            }
        }


        /// <summary>
        /// 设置实体是否被加锁。
        /// </summary>
        /// <param name="entity">实体。</param>
        /// <param name="locked">实体是否被加锁。</param>
        public void SetEntityInstanceLocked(Entity entity, bool locked)
        {
            if (entity == null)
            {
                Log.Warning("Entity is invalid.");
                return;
            }

            var entityGroup = entity.EntityGroup;
            if (entityGroup == null)
            {
                Log.Warning("Entity group is invalid.");
                return;
            }

            entityGroup.SetEntityInstanceLocked(entity.gameObject, locked);
        }

        /// <summary>
        /// 设置实体的优先级。
        /// </summary>
        /// <param name="entity">实体。</param>
        /// <param name="priority">实体优先级。</param>
        public void SetInstancePriority(Entity entity, int priority)
        {
            if (entity == null)
            {
                Log.Warning("Entity is invalid.");
                return;
            }

            var entityGroup = entity.EntityGroup;
            if (entityGroup == null)
            {
                Log.Warning("Entity group is invalid.");
                return;
            }

            entityGroup.SetEntityInstancePriority(entity.gameObject, priority);
        }


        /// <summary>
        /// 获取实体信息。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <returns>实体信息。</returns>
        private EntityInfo GetEntityInfo(int entityId)
        {
            EntityInfo entityInfo = null;
            if (m_EntityInfoDict.TryGetValue(entityId, out entityInfo))
            {
                return entityInfo;
            }

            return null;
        }

        private IEntity InternalShowEntity(int entityId, string entityAssetName, EntityGroup entityGroup,
            object entityInstance, bool isNewInstance, object userData)
        {
            var entity = m_EntityHelper.CreateEntity(entityInstance, entityGroup);
            if (entity == null)
            {
                throw new GameFrameworkException("Can not create entity in entity helper.");
            }

            var entityInfo = EntityInfo.Create(entity);
            m_EntityInfoDict.Add(entityId, entityInfo);
            entityInfo.Status = EntityStatus.WillInit;
            entity.OnInit(entityId, entityAssetName, entityGroup, isNewInstance, userData);
            entityInfo.Status = EntityStatus.Inited;
            entityGroup.AddEntity(entity);
            entityInfo.Status = EntityStatus.WillShow;
            entity.OnShow(userData);
            entityInfo.Status = EntityStatus.Showed;
            return entity;
        }

        private void InternalHideEntity(EntityInfo entityInfo, object userData)
        {
            while (entityInfo.ChildEntityCount > 0)
            {
                var childEntity = entityInfo.GetChildEntity();
                HideEntity(childEntity.Id, userData);
            }

            if (entityInfo.Status == EntityStatus.Hidden)
            {
                return;
            }

            var entity = entityInfo.Entity;
            DetachEntity(entity.Id, userData);
            entityInfo.Status = EntityStatus.WillHide;
            entity.OnHide(m_IsShutdown, userData);
            entityInfo.Status = EntityStatus.Hidden;

            var entityGroup = (EntityGroup)entity.EntityGroup;
            if (entityGroup == null)
            {
                throw new GameFrameworkException("Entity group is invalid.");
            }

            entityGroup.RemoveEntity(entity);
            if (!m_EntityInfoDict.Remove(entity.Id))
            {
                throw new GameFrameworkException("Entity info is unmanaged.");
            }

            m_RecycleQueue.Enqueue(entityInfo);
        }


        private IEntity LoadAssetSuccessCallback(string entityAssetName, Object entityAsset, InternalShowEntityInfo internalShowEntityInfo)
        {
            if (m_EntitiesToReleaseOnLoad.Contains(internalShowEntityInfo.SerialId))
            {
                m_EntitiesToReleaseOnLoad.Remove(internalShowEntityInfo.SerialId);
                ReferencePool.Release(internalShowEntityInfo);
                m_EntityHelper.ReleaseEntity(entityAsset, null);
                return null;
            }

            m_EntitiesBeingLoaded.Remove(internalShowEntityInfo.EntityId);
            var entityInstanceObject = EntityInstanceObject.Create(entityAssetName, entityAsset,
                m_EntityHelper.InstantiateEntity(entityAsset), m_EntityHelper);
            internalShowEntityInfo.EntityGroup.RegisterEntityInstanceObject(entityInstanceObject, true);

            var entity = InternalShowEntity(internalShowEntityInfo.EntityId, entityAssetName, internalShowEntityInfo.EntityGroup,
                entityInstanceObject.Target, true, internalShowEntityInfo.UserData);
            ReferencePool.Release(internalShowEntityInfo);
            return entity;
        }

        private void LoadAssetFailureCallback(string entityAssetName, string errorMessage,
            InternalShowEntityInfo internalShowEntityInfo)
        {
            if (m_EntitiesToReleaseOnLoad.Contains(internalShowEntityInfo.SerialId))
            {
                m_EntitiesToReleaseOnLoad.Remove(internalShowEntityInfo.SerialId);
                return;
            }

            m_EntitiesBeingLoaded.Remove(internalShowEntityInfo.EntityId);
            var appendErrorMessage =
                Utility.Text.Format("Load entity failure, asset name '{0}', error message '{1}'.",
                    entityAssetName, errorMessage);

            throw new GameFrameworkException(appendErrorMessage);
        }
    }
}