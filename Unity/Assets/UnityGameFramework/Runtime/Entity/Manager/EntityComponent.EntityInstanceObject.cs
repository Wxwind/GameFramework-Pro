using GFPro.ObjectPool;
using UnityEngine;

namespace GFPro.Entity
{
    public sealed partial class EntityComponent
    {
        /// <summary>
        /// 实体实例对象。
        /// </summary>
        public sealed class EntityInstanceObject : ObjectBase
        {
            private Object        m_EntityAsset;
            private IEntityHelper m_EntityHelper;

            public EntityInstanceObject()
            {
                m_EntityAsset = null;
                m_EntityHelper = null;
            }

            public static EntityInstanceObject Create(string name, Object entityAsset, Object entityInstance, IEntityHelper entityHelper)
            {
                if (entityAsset == null)
                {
                    throw new GameFrameworkException("Entity asset is invalid.");
                }

                if (entityHelper == null)
                {
                    throw new GameFrameworkException("Entity helper is invalid.");
                }

                var entityInstanceObject = ReferencePool.Acquire<EntityInstanceObject>();
                entityInstanceObject.Initialize(name, entityInstance);
                entityInstanceObject.m_EntityAsset = entityAsset;
                entityInstanceObject.m_EntityHelper = entityHelper;
                return entityInstanceObject;
            }

            public override void Clear()
            {
                base.Clear();
                m_EntityAsset = null;
                m_EntityHelper = null;
            }

            protected internal override void Release(bool isShutdown)
            {
                // 如果是关闭对象池，那么资源的销毁交给ResourceComponent.AssetObjectPool，这里不做处理
                // EntityInstanceObject负责维护PrefabAsset和实例化出来的GameObject(对象池的Target)，而AssetObject负责管理AssetHandle和Asset(对象池的Target)
                // Shutdown的时候BaseComponent带着EntityComponent带着Target一起销毁所以不用处理
                // TODO：让EntityInstanceObject早于ResourceComponent.AssetObjectPool释放？
                if (!isShutdown)
                {
                    m_EntityHelper.ReleaseEntity(m_EntityAsset, Target as Object);
                }
            }
        }
    }
}