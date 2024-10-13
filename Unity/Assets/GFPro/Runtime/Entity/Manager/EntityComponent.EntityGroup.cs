﻿using System.Collections.Generic;
using GFPro.GFObjectPool;
using UnityEngine;

namespace GFPro
{
    public sealed partial class GFEntityComponent
    {
        /// <summary>
        /// 实体组。
        /// </summary>
        public sealed class EntityGroup : IEntityGroup
        {
            private readonly string                            m_Name;
            private readonly IObjectPool<EntityInstanceObject> m_InstancePool;
            private readonly GameFrameworkLinkedList<IEntity>  m_Entities;
            private          LinkedListNode<IEntity>           m_CachedNode;
            private          GameObject                        m_root;


            /// <summary>
            /// 初始化实体组的新实例。
            /// </summary>
            /// <param name="name">实体组名称。</param>
            /// <param name="instanceAutoReleaseInterval">实体实例对象池自动释放可释放对象的间隔秒数。</param>
            /// <param name="instanceCapacity">实体实例对象池容量。</param>
            /// <param name="instanceExpireTime">实体实例对象池对象过期秒数。</param>
            /// <param name="instancePriority">实体实例对象池的优先级。</param>
            /// <param name="objectPoolComponent">对象池管理器。</param>
            public EntityGroup(string name, float instanceAutoReleaseInterval, int instanceCapacity, float instanceExpireTime, int instancePriority,
                IObjectPoolComponent objectPoolComponent, GameObject root)
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new GameFrameworkException("Entity group name is invalid.");
                }


                m_Name = name;
                m_InstancePool = objectPoolComponent.CreateSingleSpawnObjectPool<EntityInstanceObject>($"Entity Instance Pool ({name})",
                    capacity: instanceCapacity, expireTime: instanceExpireTime, priority: instancePriority);
                m_InstancePool.AutoReleaseInterval = instanceAutoReleaseInterval;
                m_Entities = new GameFrameworkLinkedList<IEntity>();
                m_CachedNode = null;
                m_root = root;
            }

            /// <summary>
            /// 获取实体组名称。
            /// </summary>
            public string Name => m_Name;

            /// <summary>
            /// 获取实体组中实体数量。
            /// </summary>
            public int EntityCount => m_Entities.Count;

            /// <summary>
            /// 获取或设置实体组实例对象池自动释放可释放对象的间隔秒数。
            /// </summary>
            public float InstanceAutoReleaseInterval
            {
                get => m_InstancePool.AutoReleaseInterval;
                set => m_InstancePool.AutoReleaseInterval = value;
            }

            /// <summary>
            /// 获取或设置实体组实例对象池的容量。
            /// </summary>
            public int InstanceCapacity
            {
                get => m_InstancePool.Capacity;
                set => m_InstancePool.Capacity = value;
            }

            /// <summary>
            /// 获取或设置实体组实例对象池对象过期秒数。
            /// </summary>
            public float InstanceExpireTime
            {
                get => m_InstancePool.ExpireTime;
                set => m_InstancePool.ExpireTime = value;
            }

            /// <summary>
            /// 获取或设置实体组实例对象池的优先级。
            /// </summary>
            public int InstancePriority
            {
                get => m_InstancePool.Priority;
                set => m_InstancePool.Priority = value;
            }

            public GameObject Root => m_root;


            /// <summary>
            /// 实体组轮询。
            /// </summary>
            /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
            /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
            public void Update(float elapseSeconds, float realElapseSeconds)
            {
                var current = m_Entities.First;
                while (current != null)
                {
                    m_CachedNode = current.Next;
                    current.Value.OnUpdate(elapseSeconds, realElapseSeconds);
                    current = m_CachedNode;
                    m_CachedNode = null;
                }
            }

            /// <summary>
            /// 实体组中是否存在实体。
            /// </summary>
            /// <param name="entityId">实体序列编号。</param>
            /// <returns>实体组中是否存在实体。</returns>
            public bool HasEntity(int entityId)
            {
                foreach (var entity in m_Entities)
                {
                    if (entity.Id == entityId)
                    {
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// 实体组中是否存在实体。
            /// </summary>
            /// <param name="entityAssetName">实体资源名称。</param>
            /// <returns>实体组中是否存在实体。</returns>
            public bool HasEntity(string entityAssetName)
            {
                if (string.IsNullOrEmpty(entityAssetName))
                {
                    throw new GameFrameworkException("Entity asset name is invalid.");
                }

                foreach (var entity in m_Entities)
                {
                    if (entity.EntityAssetName == entityAssetName)
                    {
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// 从实体组中获取实体。
            /// </summary>
            /// <param name="entityId">实体序列编号。</param>
            /// <returns>要获取的实体。</returns>
            public IEntity GetEntity(int entityId)
            {
                foreach (var entity in m_Entities)
                {
                    if (entity.Id == entityId)
                    {
                        return entity;
                    }
                }

                return null;
            }

            /// <summary>
            /// 从实体组中获取实体。
            /// </summary>
            /// <param name="entityAssetName">实体资源名称。</param>
            /// <returns>要获取的实体。</returns>
            public IEntity GetEntity(string entityAssetName)
            {
                if (string.IsNullOrEmpty(entityAssetName))
                {
                    throw new GameFrameworkException("Entity asset name is invalid.");
                }

                foreach (var entity in m_Entities)
                {
                    if (entity.EntityAssetName == entityAssetName)
                    {
                        return entity;
                    }
                }

                return null;
            }

            /// <summary>
            /// 从实体组中获取实体。
            /// </summary>
            /// <param name="entityAssetName">实体资源名称。</param>
            /// <returns>要获取的实体。</returns>
            public IEntity[] GetEntities(string entityAssetName)
            {
                if (string.IsNullOrEmpty(entityAssetName))
                {
                    throw new GameFrameworkException("Entity asset name is invalid.");
                }

                var results = new List<IEntity>();
                foreach (var entity in m_Entities)
                {
                    if (entity.EntityAssetName == entityAssetName)
                    {
                        results.Add(entity);
                    }
                }

                return results.ToArray();
            }

            /// <summary>
            /// 从实体组中获取实体。
            /// </summary>
            /// <param name="entityAssetName">实体资源名称。</param>
            /// <param name="results">要获取的实体。</param>
            public void GetEntities(string entityAssetName, List<IEntity> results)
            {
                if (string.IsNullOrEmpty(entityAssetName))
                {
                    throw new GameFrameworkException("Entity asset name is invalid.");
                }

                if (results == null)
                {
                    throw new GameFrameworkException("Results is invalid.");
                }

                results.Clear();
                foreach (var entity in m_Entities)
                {
                    if (entity.EntityAssetName == entityAssetName)
                    {
                        results.Add(entity);
                    }
                }
            }

            /// <summary>
            /// 从实体组中获取所有实体。
            /// </summary>
            /// <returns>实体组中的所有实体。</returns>
            public IEntity[] GetAllEntities()
            {
                var results = new List<IEntity>();
                foreach (var entity in m_Entities)
                {
                    results.Add(entity);
                }

                return results.ToArray();
            }

            /// <summary>
            /// 从实体组中获取所有实体。
            /// </summary>
            /// <param name="results">实体组中的所有实体。</param>
            public void GetAllEntities(List<IEntity> results)
            {
                if (results == null)
                {
                    throw new GameFrameworkException("Results is invalid.");
                }

                results.Clear();
                foreach (var entity in m_Entities)
                {
                    results.Add(entity);
                }
            }

            /// <summary>
            /// 往实体组增加实体。
            /// </summary>
            /// <param name="entity">要增加的实体。</param>
            public void AddEntity(IEntity entity)
            {
                m_Entities.AddLast(entity);
            }

            /// <summary>
            /// 从实体组移除实体。
            /// </summary>
            /// <param name="entity">要移除的实体。</param>
            public void RemoveEntity(IEntity entity)
            {
                if (m_CachedNode != null && m_CachedNode.Value == entity)
                {
                    m_CachedNode = m_CachedNode.Next;
                }

                if (!m_Entities.Remove(entity))
                {
                    throw new GameFrameworkException($"Entity group '{m_Name}' not exists specified entity '[{entity.Id}]{entity.EntityAssetName}'.");
                }
            }

            public void RegisterEntityInstanceObject(EntityInstanceObject obj, bool spawned)
            {
                m_InstancePool.Register(obj, spawned);
            }

            public EntityInstanceObject SpawnEntityInstanceObject(string name)
            {
                return m_InstancePool.Spawn(name);
            }

            public void UnspawnEntity(IEntity entity)
            {
                m_InstancePool.Unspawn(entity.Handle);
            }

            public void SetEntityInstanceLocked(object entityInstance, bool locked)
            {
                if (entityInstance == null)
                {
                    throw new GameFrameworkException("Entity instance is invalid.");
                }

                m_InstancePool.SetLocked(entityInstance, locked);
            }

            public void SetEntityInstancePriority(object entityInstance, int priority)
            {
                if (entityInstance == null)
                {
                    throw new GameFrameworkException("Entity instance is invalid.");
                }

                m_InstancePool.SetPriority(entityInstance, priority);
            }
        }
    }
}