﻿using UnityEngine;

namespace GFPro.Entity
{
    /// <summary>
    /// 实体辅助器接口。
    /// </summary>
    public interface IEntityHelper
    {
        /// <summary>
        /// 实例化实体。
        /// </summary>
        /// <param name="entityAsset">要实例化的实体资源。</param>
        /// <returns>实例化后的实体。</returns>
        Object InstantiateEntity(Object entityAsset);

        /// <summary>
        /// 创建实体。
        /// </summary>
        /// <param name="entityInstance">实体实例。</param>
        /// <param name="entityGroup">实体所属的实体组。</param>
        /// <returns>实体。</returns>
        IEntity CreateEntity(object entityInstance, IEntityGroup entityGroup);

        /// <summary>
        /// 释放实体。
        /// </summary>
        /// <param name="entityAsset">要释放的实体资源。</param>
        /// <param name="entityInstance">要释放的实体实例。</param>
        void ReleaseEntity(Object entityAsset, Object entityInstance);
    }
}