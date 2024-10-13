using GFPro.ObjectPool;
using UnityEngine;

namespace GFPro.UI
{
    internal sealed partial class UIManager
    {
        /// <summary>
        /// 界面实例对象。
        /// </summary>
        private sealed class UIFormInstanceObject : ObjectBase
        {
            private Object        m_UIFormAsset;
            private IUIFormHelper m_UIFormHelper;

            public UIFormInstanceObject()
            {
                m_UIFormAsset = null;
                m_UIFormHelper = null;
            }

            public static UIFormInstanceObject Create(string name, Object uiFormAsset, object uiFormInstance, IUIFormHelper uiFormHelper)
            {
                if (uiFormAsset == null)
                {
                    throw new GameFrameworkException("UI form asset is invalid.");
                }

                if (uiFormHelper == null)
                {
                    throw new GameFrameworkException("UI form helper is invalid.");
                }

                var uiFormInstanceObject = ReferencePool.Acquire<UIFormInstanceObject>();
                uiFormInstanceObject.Initialize(name, uiFormInstance);
                uiFormInstanceObject.m_UIFormAsset = uiFormAsset;
                uiFormInstanceObject.m_UIFormHelper = uiFormHelper;
                return uiFormInstanceObject;
            }

            public override void Clear()
            {
                base.Clear();
                m_UIFormAsset = null;
                m_UIFormHelper = null;
            }

            protected internal override void Release(bool isShutdown)
            {
                // 如果是关闭对象池，那么资源的销毁交给ResourceComponent.AssetObjectPool，这里不做处理
                // UIFormInstanceObject负责维护PrefabAsset和实例化出来的GameObject(对象池的Target)，而AssetObject负责管理AssetHandle和Asset(对象池的Target)
                // Shutdown的时候BaseComponent带着EntityComponent带着Target一起销毁所以不用处理
                // TODO：让释放UIFormInstanceObject晚于ResourceComponent.AssetObjectPool释放？
                if (!isShutdown)
                {
                    m_UIFormHelper.ReleaseUIForm(m_UIFormAsset, Target as GameObject);
                }
            }
        }
    }
}