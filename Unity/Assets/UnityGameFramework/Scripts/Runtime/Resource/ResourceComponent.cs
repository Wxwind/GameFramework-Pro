﻿using System;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.Resource;
using UnityEngine;
using YooAsset;
using Object = UnityEngine.Object;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    ///     资源组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Resource")]
    public sealed class ResourceComponent : GameFrameworkComponent
    {
        private const            string           DefaultPackage                  = "DefaultPackage";
        [SerializeField] private float            m_MinUnloadUnusedAssetsInterval = 60f;
        [SerializeField] private float            m_MaxUnloadUnusedAssetsInterval = 300f;
        private                  AsyncOperation   m_AsyncOperation;
        private                  bool             m_ForceUnloadUnusedAssets;
        private                  float            m_LastUnloadUnusedAssetsOperationElapseSeconds;
        private                  bool             m_PerformGCCollect;
        private                  bool             m_PreorderUnloadUnusedAssets;
        private                  IResourceManager m_ResourceManager;


        /// <summary>
        /// 获取无用资源释放的等待时长，以秒为单位。
        /// </summary>
        public float LastUnloadUnusedAssetsOperationElapseSeconds => m_LastUnloadUnusedAssetsOperationElapseSeconds;

        /// <summary>
        ///     获取或设置无用资源释放的最小间隔时间，以秒为单位。
        /// </summary>
        public float MinUnloadUnusedAssetsInterval
        {
            get => m_MinUnloadUnusedAssetsInterval;
            set => m_MinUnloadUnusedAssetsInterval = value;
        }

        /// <summary>
        ///     获取或设置无用资源释放的最大间隔时间，以秒为单位。
        /// </summary>
        public float MaxUnloadUnusedAssetsInterval
        {
            get => m_MaxUnloadUnusedAssetsInterval;
            set => m_MaxUnloadUnusedAssetsInterval = value;
        }

        public string ApplicableGameVersion { get; set; }

        private void Start()
        {
            m_ResourceManager = GameFrameworkEntry.GetModule<IResourceManager>();
            if (m_ResourceManager == null)
            {
                Log.Fatal("Resource manager is invalid.");
                return;
            }


            m_ResourceManager.Initialize();
        }

        private void Update()
        {
            m_LastUnloadUnusedAssetsOperationElapseSeconds += Time.unscaledDeltaTime;
            if (m_AsyncOperation == null && (m_ForceUnloadUnusedAssets ||
                                             m_LastUnloadUnusedAssetsOperationElapseSeconds >=
                                             m_MaxUnloadUnusedAssetsInterval || (m_PreorderUnloadUnusedAssets &&
                                                                                 m_LastUnloadUnusedAssetsOperationElapseSeconds >= m_MinUnloadUnusedAssetsInterval)))
            {
                Log.Info("Unload unused assets...");
                m_ForceUnloadUnusedAssets = false;
                m_PreorderUnloadUnusedAssets = false;
                m_LastUnloadUnusedAssetsOperationElapseSeconds = 0f;
                m_AsyncOperation = Resources.UnloadUnusedAssets();
                m_ResourceManager.UnloadUnusedAssets();
            }

            if (m_AsyncOperation != null && m_AsyncOperation.isDone)
            {
                m_AsyncOperation = null;
                if (m_PerformGCCollect)
                {
                    Log.Info("GC.Collect...");
                    m_PerformGCCollect = false;
                    GC.Collect();
                }
            }
        }

        public async UniTask<InitializationOperation> InitPackage(ResourceMode mode, string packageName = "")
        {
            if (m_ResourceManager == null)
            {
                Log.Fatal("Resource manager is invalid.");
                return null;
            }

            return await m_ResourceManager.InitPackage(mode, string.IsNullOrEmpty(packageName)
                ? DefaultPackage
                : packageName);
        }

        public UniTask<T> LoadAssetAsync<T>(string location, string packageName = "", Action<float> progress = null)
            where T : Object
        {
            return m_ResourceManager.LoadAssetAsync<T>(location, packageName, progress);
        }


        public void UnloadAsset(Object asset)
        {
            m_ResourceManager.UnloadAsset(asset);
        }


        /// <summary>
        ///     预订执行释放未被使用的资源。
        /// </summary>
        /// <param name="performGCCollect">是否使用垃圾回收。</param>
        public void UnloadUnusedAssets(bool performGCCollect)
        {
            m_PreorderUnloadUnusedAssets = true;
            if (performGCCollect) m_PerformGCCollect = true;
        }

        /// <summary>
        ///     强制执行释放未被使用的资源。
        /// </summary>
        /// <param name="performGCCollect">是否使用垃圾回收。</param>
        public void ForceUnloadUnusedAssets(bool performGCCollect)
        {
            m_ForceUnloadUnusedAssets = true;
            if (performGCCollect) m_PerformGCCollect = true;
        }
    }
}