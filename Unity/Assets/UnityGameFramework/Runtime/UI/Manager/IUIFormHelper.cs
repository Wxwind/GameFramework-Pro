﻿using UnityEngine;

namespace GFPro.UI
{
    /// <summary>
    /// 界面辅助器接口。
    /// </summary>
    public interface IUIFormHelper
    {
        /// <summary>
        /// 实例化界面。
        /// </summary>
        /// <param name="uiFormAsset">要实例化的界面资源。</param>
        /// <returns>实例化后的界面。</returns>
        object InstantiateUIForm(Object uiFormAsset);

        /// <summary>
        /// 创建界面。
        /// </summary>
        /// <param name="uiFormInstance">界面实例。</param>
        /// <param name="uiGroup">界面所属的界面组。</param>
        /// <returns>界面。</returns>
        IUIForm CreateUIForm(GameObject uiFormInstance, IUIGroup uiGroup);

        /// <summary>
        /// 释放界面。
        /// </summary>
        /// <param name="uiFormAsset">要释放的界面资源。</param>
        /// <param name="uiFormInstance">要释放的界面实例。</param>
        void ReleaseUIForm(Object uiFormAsset, GameObject uiFormInstance);
    }
}