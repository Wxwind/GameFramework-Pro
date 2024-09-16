using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityGameFramework.ObjectPool;
using UnityGameFramework.Resource;
using Object = UnityEngine.Object;

namespace UnityGameFramework.UI
{
    /// <summary>
    /// 界面管理器。
    /// </summary>
    internal sealed partial class UIManager : IUIManager
    {
        private readonly Dictionary<string, UIGroup>       m_UIGroups;
        private readonly Dictionary<int, string>           m_UIFormsBeingLoaded;
        private readonly HashSet<int>                      m_UIFormsToReleaseOnLoad;
        private readonly Queue<IUIForm>                    m_RecycleQueue;
        private          IObjectPoolComponent              m_ObjectPoolManager;
        private          IResourceComponent                m_ResourceComponent;
        private          IObjectPool<UIFormInstanceObject> m_InstancePool;
        private          IUIFormHelper                     m_UIFormHelper;
        private          int                               m_Serial;
        private          bool                              m_IsShutdown;

        /// <summary>
        /// 初始化界面管理器的新实例。
        /// </summary>
        public UIManager()
        {
            m_UIGroups = new Dictionary<string, UIGroup>(StringComparer.Ordinal);
            m_UIFormsBeingLoaded = new Dictionary<int, string>();
            m_UIFormsToReleaseOnLoad = new HashSet<int>();
            m_RecycleQueue = new Queue<IUIForm>();
            m_ObjectPoolManager = null;
            m_ResourceComponent = null;
            m_InstancePool = null;
            m_UIFormHelper = null;
            m_Serial = 0;
            m_IsShutdown = false;
        }

        /// <summary>
        /// 获取界面组数量。
        /// </summary>
        public int UIGroupCount => m_UIGroups.Count;

        /// <summary>
        /// 获取或设置界面实例对象池自动释放可释放对象的间隔秒数。
        /// </summary>
        public float InstanceAutoReleaseInterval
        {
            get => m_InstancePool.AutoReleaseInterval;
            set => m_InstancePool.AutoReleaseInterval = value;
        }

        /// <summary>
        /// 获取或设置界面实例对象池的容量。
        /// </summary>
        public int InstanceCapacity
        {
            get => m_InstancePool.Capacity;
            set => m_InstancePool.Capacity = value;
        }

        /// <summary>
        /// 获取或设置界面实例对象池对象过期秒数。
        /// </summary>
        public float InstanceExpireTime
        {
            get => m_InstancePool.ExpireTime;
            set => m_InstancePool.ExpireTime = value;
        }

        /// <summary>
        /// 获取或设置界面实例对象池的优先级。
        /// </summary>
        public int InstancePriority
        {
            get => m_InstancePool.Priority;
            set => m_InstancePool.Priority = value;
        }

        /// <summary>
        /// 界面管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            while (m_RecycleQueue.Count > 0)
            {
                var uiForm = m_RecycleQueue.Dequeue();
                uiForm.OnRecycle();
                m_InstancePool.Unspawn(uiForm.Handle);
            }

            foreach (var uiGroup in m_UIGroups)
            {
                uiGroup.Value.Update(elapseSeconds, realElapseSeconds);
            }
        }

        /// <summary>
        /// 关闭并清理界面管理器。
        /// </summary>
        public void Shutdown()
        {
            m_IsShutdown = true;
            CloseAllLoadedUIForms();
            m_UIGroups.Clear();
            m_UIFormsBeingLoaded.Clear();
            m_UIFormsToReleaseOnLoad.Clear();
            m_RecycleQueue.Clear();
        }

        /// <summary>
        /// 设置对象池管理器。
        /// </summary>
        /// <param name="objectPoolManager">对象池管理器。</param>
        public void SetObjectPoolManager(IObjectPoolComponent objectPoolManager)
        {
            if (objectPoolManager == null)
            {
                throw new GameFrameworkException("Object pool manager is invalid.");
            }

            m_ObjectPoolManager = objectPoolManager;
            m_InstancePool = m_ObjectPoolManager.CreateSingleSpawnObjectPool<UIFormInstanceObject>("UI Instance Pool");
        }

        /// <summary>
        /// 设置资源管理器。
        /// </summary>
        /// <param name="resourceComponent">资源管理器。</param>
        public void SetResourceManager(IResourceComponent resourceComponent)
        {
            if (resourceComponent == null)
            {
                throw new GameFrameworkException("Resource manager is invalid.");
            }

            m_ResourceComponent = resourceComponent;
        }

        /// <summary>
        /// 设置界面辅助器。
        /// </summary>
        /// <param name="uiFormHelper">界面辅助器。</param>
        public void SetUIFormHelper(IUIFormHelper uiFormHelper)
        {
            if (uiFormHelper == null)
            {
                throw new GameFrameworkException("UI form helper is invalid.");
            }

            m_UIFormHelper = uiFormHelper;
        }

        /// <summary>
        /// 是否存在界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>是否存在界面组。</returns>
        public bool HasUIGroup(string uiGroupName)
        {
            if (string.IsNullOrEmpty(uiGroupName))
            {
                throw new GameFrameworkException("UI group name is invalid.");
            }

            return m_UIGroups.ContainsKey(uiGroupName);
        }

        /// <summary>
        /// 获取界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>要获取的界面组。</returns>
        public IUIGroup GetUIGroup(string uiGroupName)
        {
            if (string.IsNullOrEmpty(uiGroupName))
            {
                throw new GameFrameworkException("UI group name is invalid.");
            }

            UIGroup uiGroup = null;
            if (m_UIGroups.TryGetValue(uiGroupName, out uiGroup))
            {
                return uiGroup;
            }

            return null;
        }

        /// <summary>
        /// 获取所有界面组。
        /// </summary>
        /// <returns>所有界面组。</returns>
        public IUIGroup[] GetAllUIGroups()
        {
            var index = 0;
            var results = new IUIGroup[m_UIGroups.Count];
            foreach (var uiGroup in m_UIGroups)
            {
                results[index++] = uiGroup.Value;
            }

            return results;
        }

        /// <summary>
        /// 获取所有界面组。
        /// </summary>
        /// <param name="results">所有界面组。</param>
        public void GetAllUIGroups(List<IUIGroup> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (var uiGroup in m_UIGroups)
            {
                results.Add(uiGroup.Value);
            }
        }

        /// <summary>
        /// 增加界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="uiGroupHelper">界面组辅助器。</param>
        /// <returns>是否增加界面组成功。</returns>
        public bool AddUIGroup(string uiGroupName, IUIGroupHelper uiGroupHelper)
        {
            return AddUIGroup(uiGroupName, 0, uiGroupHelper);
        }

        /// <summary>
        /// 增加界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="uiGroupDepth">界面组深度。</param>
        /// <param name="uiGroupHelper">界面组辅助器。</param>
        /// <returns>是否增加界面组成功。</returns>
        public bool AddUIGroup(string uiGroupName, int uiGroupDepth, IUIGroupHelper uiGroupHelper)
        {
            if (string.IsNullOrEmpty(uiGroupName))
            {
                throw new GameFrameworkException("UI group name is invalid.");
            }

            if (uiGroupHelper == null)
            {
                throw new GameFrameworkException("UI group helper is invalid.");
            }

            if (HasUIGroup(uiGroupName))
            {
                return false;
            }

            m_UIGroups.Add(uiGroupName, new UIGroup(uiGroupName, uiGroupDepth, uiGroupHelper));

            return true;
        }

        /// <summary>
        /// 是否存在界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>是否存在界面。</returns>
        public bool HasUIForm(int serialId)
        {
            foreach (var uiGroup in m_UIGroups)
            {
                if (uiGroup.Value.HasUIForm(serialId))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 是否存在界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>是否存在界面。</returns>
        public bool HasUIForm(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new GameFrameworkException("UI form asset name is invalid.");
            }

            foreach (var uiGroup in m_UIGroups)
            {
                if (uiGroup.Value.HasUIForm(uiFormAssetName))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>要获取的界面。</returns>
        public IUIForm GetUIForm(int serialId)
        {
            foreach (var uiGroup in m_UIGroups)
            {
                var uiForm = uiGroup.Value.GetUIForm(serialId);
                if (uiForm != null)
                {
                    return uiForm;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        public IUIForm GetUIForm(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new GameFrameworkException("UI form asset name is invalid.");
            }

            foreach (var uiGroup in m_UIGroups)
            {
                var uiForm = uiGroup.Value.GetUIForm(uiFormAssetName);
                if (uiForm != null)
                {
                    return uiForm;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        public IUIForm[] GetUIForms(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new GameFrameworkException("UI form asset name is invalid.");
            }

            var results = new List<IUIForm>();
            foreach (var uiGroup in m_UIGroups)
            {
                results.AddRange(uiGroup.Value.GetUIForms(uiFormAssetName));
            }

            return results.ToArray();
        }

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="results">要获取的界面。</param>
        public void GetUIForms(string uiFormAssetName, List<IUIForm> results)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new GameFrameworkException("UI form asset name is invalid.");
            }

            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (var uiGroup in m_UIGroups)
            {
                uiGroup.Value.InternalGetUIForms(uiFormAssetName, results);
            }
        }

        /// <summary>
        /// 获取所有已加载的界面。
        /// </summary>
        /// <returns>所有已加载的界面。</returns>
        public IUIForm[] GetAllLoadedUIForms()
        {
            var results = new List<IUIForm>();
            foreach (var uiGroup in m_UIGroups)
            {
                results.AddRange(uiGroup.Value.GetAllUIForms());
            }

            return results.ToArray();
        }

        /// <summary>
        /// 获取所有已加载的界面。
        /// </summary>
        /// <param name="results">所有已加载的界面。</param>
        public void GetAllLoadedUIForms(List<IUIForm> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (var uiGroup in m_UIGroups)
            {
                uiGroup.Value.InternalGetAllUIForms(results);
            }
        }

        /// <summary>
        /// 获取所有正在加载界面的序列编号。
        /// </summary>
        /// <returns>所有正在加载界面的序列编号。</returns>
        public int[] GetAllLoadingUIFormSerialIds()
        {
            var index = 0;
            var results = new int[m_UIFormsBeingLoaded.Count];
            foreach (var uiFormBeingLoaded in m_UIFormsBeingLoaded)
            {
                results[index++] = uiFormBeingLoaded.Key;
            }

            return results;
        }

        /// <summary>
        /// 获取所有正在加载界面的序列编号。
        /// </summary>
        /// <param name="results">所有正在加载界面的序列编号。</param>
        public void GetAllLoadingUIFormSerialIds(List<int> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (var uiFormBeingLoaded in m_UIFormsBeingLoaded)
            {
                results.Add(uiFormBeingLoaded.Key);
            }
        }

        /// <summary>
        /// 是否正在加载界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>是否正在加载界面。</returns>
        public bool IsLoadingUIForm(int serialId)
        {
            return m_UIFormsBeingLoaded.ContainsKey(serialId);
        }

        /// <summary>
        /// 是否正在加载界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>是否正在加载界面。</returns>
        public bool IsLoadingUIForm(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new GameFrameworkException("UI form asset name is invalid.");
            }

            return m_UIFormsBeingLoaded.ContainsValue(uiFormAssetName);
        }

        /// <summary>
        /// 是否是合法的界面。
        /// </summary>
        /// <param name="uiForm">界面。</param>
        /// <returns>界面是否合法。</returns>
        public bool IsValidUIForm(IUIForm uiForm)
        {
            if (uiForm == null)
            {
                return false;
            }

            return HasUIForm(uiForm.SerialId);
        }


        public UniTask<IUIForm> OpenUIForm(string uiFormAssetName, string uiGroupName, object userData = null)
        {
            return OpenUIForm(uiFormAssetName, uiGroupName, false, userData);
        }


        public async UniTask<IUIForm> OpenUIForm(string uiFormAssetName, string uiGroupName,
            bool pauseCoveredUIForm, object userData = null)
        {
            if (m_ResourceComponent == null)
            {
                throw new GameFrameworkException("You must set resource manager first.");
            }

            if (m_UIFormHelper == null)
            {
                throw new GameFrameworkException("You must set UI form helper first.");
            }

            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new GameFrameworkException("UI form asset name is invalid.");
            }

            if (string.IsNullOrEmpty(uiGroupName))
            {
                throw new GameFrameworkException("UI group name is invalid.");
            }

            var uiGroup = (UIGroup)GetUIGroup(uiGroupName);
            if (uiGroup == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("UI group '{0}' is not exist.", uiGroupName));
            }

            var serialId = ++m_Serial;
            var uiFormInstanceObject = m_InstancePool.Spawn(uiFormAssetName);
            var info = OpenUIFormInfo.Create(serialId, uiGroup, pauseCoveredUIForm);
            if (uiFormInstanceObject == null)
            {
                m_UIFormsBeingLoaded.Add(serialId, uiFormAssetName);
                try
                {
                    var asset = await m_ResourceComponent.LoadAssetAsync<Object>(uiFormAssetName);
                    return LoadAssetSuccessCallback(uiFormAssetName, asset, info, userData);
                }
                catch (Exception e)
                {
                    LoadAssetFailureCallback(uiFormAssetName, e.Message, info);
                    return null;
                }
            }

            return InternalOpenUIForm(serialId, uiFormAssetName, uiGroup, uiFormInstanceObject.Target as GameObject,
                pauseCoveredUIForm,
                false, userData);
        }


        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="serialId">要关闭界面的序列编号。</param>
        public void CloseUIForm(int serialId)
        {
            if (IsLoadingUIForm(serialId))
            {
                m_UIFormsToReleaseOnLoad.Add(serialId);
                m_UIFormsBeingLoaded.Remove(serialId);
                return;
            }

            var uiForm = GetUIForm(serialId);
            if (uiForm == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find UI form '{0}'.", serialId));
            }

            CloseUIForm(uiForm);
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="uiForm">要关闭的界面。</param>
        public void CloseUIForm(IUIForm uiForm)
        {
            if (uiForm == null)
            {
                throw new GameFrameworkException("UI form is invalid.");
            }

            var uiGroup = (UIGroup)uiForm.UIGroup;
            if (uiGroup == null)
            {
                throw new GameFrameworkException("UI group is invalid.");
            }

            uiGroup.RemoveUIForm(uiForm);
            uiForm.OnClose(m_IsShutdown);
            uiGroup.Refresh();

            m_RecycleQueue.Enqueue(uiForm);
        }


        /// <summary>
        /// 关闭所有已加载的界面。
        /// </summary>
        public void CloseAllLoadedUIForms()
        {
            var uiForms = GetAllLoadedUIForms();
            foreach (var uiForm in uiForms)
            {
                if (!HasUIForm(uiForm.SerialId))
                {
                    continue;
                }

                CloseUIForm(uiForm);
            }
        }

        /// <summary>
        /// 关闭所有正在加载的界面。
        /// </summary>
        public void CloseAllLoadingUIForms()
        {
            foreach (var uiFormBeingLoaded in m_UIFormsBeingLoaded)
            {
                m_UIFormsToReleaseOnLoad.Add(uiFormBeingLoaded.Key);
            }

            m_UIFormsBeingLoaded.Clear();
        }


        /// <summary>
        /// 激活界面。
        /// </summary>
        /// <param name="uiForm">要激活的界面。</param>
        public void RefocusUIForm(IUIForm uiForm)
        {
            if (uiForm == null)
            {
                throw new GameFrameworkException("UI form is invalid.");
            }

            var uiGroup = (UIGroup)uiForm.UIGroup;
            if (uiGroup == null)
            {
                throw new GameFrameworkException("UI group is invalid.");
            }

            uiGroup.RefocusUIForm(uiForm);
            uiGroup.Refresh();
            uiForm.OnRefocus();
        }

        /// <summary>
        /// 设置界面实例是否被加锁。
        /// </summary>
        /// <param name="uiFormInstance">要设置是否被加锁的界面实例。</param>
        /// <param name="locked">界面实例是否被加锁。</param>
        public void SetUIFormInstanceLocked(object uiFormInstance, bool locked)
        {
            if (uiFormInstance == null)
            {
                throw new GameFrameworkException("UI form instance is invalid.");
            }

            m_InstancePool.SetLocked(uiFormInstance, locked);
        }

        /// <summary>
        /// 设置界面实例的优先级。
        /// </summary>
        /// <param name="uiFormInstance">要设置优先级的界面实例。</param>
        /// <param name="priority">界面实例优先级。</param>
        public void SetUIFormInstancePriority(object uiFormInstance, int priority)
        {
            if (uiFormInstance == null)
            {
                throw new GameFrameworkException("UI form instance is invalid.");
            }

            m_InstancePool.SetPriority(uiFormInstance, priority);
        }

        private IUIForm InternalOpenUIForm(int serialId, string uiFormAssetName, UIGroup uiGroup, GameObject uiFormInstance,
            bool pauseCoveredUIForm, bool isNewInstance, object userData)
        {
            var uiForm = m_UIFormHelper.CreateUIForm(uiFormInstance, uiGroup);
            if (uiForm == null)
            {
                throw new GameFrameworkException("Can not create UI form in UI form helper.");
            }

            uiForm.OnInit(serialId, uiFormAssetName, uiGroup, pauseCoveredUIForm, isNewInstance);
            uiGroup.AddUIForm(uiForm);
            uiForm.OnOpen(userData);
            uiGroup.Refresh();
            return uiForm;
        }

        private IUIForm LoadAssetSuccessCallback(string uiFormAssetName, Object uiFormAsset,
            OpenUIFormInfo openUIFormInfo, object userData)
        {
            if (m_UIFormsToReleaseOnLoad.Contains(openUIFormInfo.SerialId))
            {
                m_UIFormsToReleaseOnLoad.Remove(openUIFormInfo.SerialId);
                ReferencePool.Release(openUIFormInfo);
                m_UIFormHelper.ReleaseUIForm(uiFormAsset, null);
                return null;
            }

            m_UIFormsBeingLoaded.Remove(openUIFormInfo.SerialId);
            var uiFormInstanceObject = UIFormInstanceObject.Create(uiFormAssetName, uiFormAsset,
                m_UIFormHelper.InstantiateUIForm(uiFormAsset), m_UIFormHelper);
            m_InstancePool.Register(uiFormInstanceObject, true);

            var uiForm = InternalOpenUIForm(openUIFormInfo.SerialId, uiFormAssetName, openUIFormInfo.UIGroup,
                uiFormInstanceObject.Target as GameObject, openUIFormInfo.PauseCoveredUIForm, true, userData);
            ReferencePool.Release(openUIFormInfo);
            return uiForm;
        }

        private void LoadAssetFailureCallback(string uiFormAssetName, string errorMessage,
            OpenUIFormInfo openUIFormInfo)
        {
            if (m_UIFormsToReleaseOnLoad.Contains(openUIFormInfo.SerialId))
            {
                m_UIFormsToReleaseOnLoad.Remove(openUIFormInfo.SerialId);
                return;
            }

            m_UIFormsBeingLoaded.Remove(openUIFormInfo.SerialId);
            var appendErrorMessage =
                Utility.Text.Format("Load UI form failure, asset name '{0}', error message '{1}'.",
                    uiFormAssetName, errorMessage);

            throw new GameFrameworkException(appendErrorMessage);
        }
    }
}