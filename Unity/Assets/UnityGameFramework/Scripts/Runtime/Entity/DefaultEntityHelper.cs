using GameFramework.Entity;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 默认实体辅助器。
    /// </summary>
    public class DefaultEntityHelper : IEntityHelper
    {
        private ResourceComponent m_ResourceComponent;

        public DefaultEntityHelper()
        {
            m_ResourceComponent = GameEntry.GetComponent<ResourceComponent>();
            if (m_ResourceComponent == null)
            {
                Log.Fatal("Resource component is invalid.");
            }
        }

        /// <summary>
        /// 实例化实体。
        /// </summary>
        /// <param name="entityAsset">要实例化的实体资源。</param>
        /// <returns>实例化后的实体。</returns>
        public Object InstantiateEntity(Object entityAsset)
        {
            return Object.Instantiate(entityAsset);
        }

        /// <summary>
        /// 创建实体。
        /// </summary>
        /// <param name="entityInstance">实体实例。</param>
        /// <param name="entityGroup">实体所属的实体组。</param>
        /// <returns>实体。</returns>
        public IEntity CreateEntity(object entityInstance, IEntityGroup entityGroup)
        {
            var gameObject = entityInstance as GameObject;
            if (gameObject == null)
            {
                Log.Error("Entity instance is invalid.");
                return null;
            }

            var transform = gameObject.transform;
            transform.SetParent(((MonoBehaviour)entityGroup.Helper).transform);

            return gameObject.GetOrAddComponent<Entity>();
        }

        /// <summary>
        /// 释放实体。
        /// </summary>
        /// <param name="entityAsset">要释放的实体资源。</param>
        /// <param name="entityInstance">要释放的实体实例。</param>
        public void ReleaseEntity(Object entityAsset, Object entityInstance)
        {
            m_ResourceComponent.UnloadAsset(entityAsset);
            Object.Destroy(entityInstance);
        }
    }
}