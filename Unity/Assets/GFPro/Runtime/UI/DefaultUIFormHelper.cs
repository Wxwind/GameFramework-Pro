using GFPro.UI;
using UnityEngine;

namespace GFPro
{
    /// <summary>
    /// 默认界面辅助器。
    /// </summary>
    public class DefaultUIFormHelper : IUIFormHelper
    {
        /// <summary>
        /// 实例化界面。
        /// </summary>
        /// <param name="uiFormAsset">要实例化的界面资源。</param>
        /// <returns>实例化后的界面。</returns>
        public object InstantiateUIForm(UnityEngine.Object uiFormAsset)
        {
            return UnityEngine.Object.Instantiate(uiFormAsset);
        }

        /// <summary>
        /// 创建界面。
        /// </summary>
        /// <param name="uiFormInstance">界面实例。</param>
        /// <param name="uiGroup">界面所属的界面组。</param>
        /// <returns>界面。</returns>
        public IUIForm CreateUIForm(GameObject uiFormInstance, IUIGroup uiGroup)
        {
            var gameObject = uiFormInstance;
            if (gameObject == null)
            {
                Log.Error("UI form instance is invalid.");
                return null;
            }

            var transform = gameObject.transform;
            transform.SetParent(((MonoBehaviour)uiGroup.Helper).transform);
            transform.localScale = Vector3.one;

            return gameObject.GetOrAddComponent<UIForm>();
        }

        /// <summary>
        /// 释放界面。
        /// </summary>
        /// <param name="uiFormAsset">要释放的界面资源。</param>
        /// <param name="uiFormInstance">要释放的界面实例。</param>
        public void ReleaseUIForm(UnityEngine.Object uiFormAsset, GameObject uiFormInstance)
        {
            ResourceComponent.Instance.UnloadAsset(uiFormAsset);
            UnityEngine.Object.Destroy(uiFormInstance);
        }
    }
}