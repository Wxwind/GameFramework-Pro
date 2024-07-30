﻿using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFramework.Resource;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;

namespace GameFramework.Scene
{
    /// <summary>
    /// 场景管理器。
    /// </summary>
    internal sealed class SceneManager : GameFrameworkModule, ISceneManager
    {
        private readonly List<string>     m_LoadedSceneAssetNames;
        private readonly List<string>     m_LoadingSceneAssetNames;
        private readonly List<string>     m_UnloadingSceneAssetNames;
        private          IResourceManager m_ResourceManager;


        /// <summary>
        /// 初始化场景管理器的新实例。
        /// </summary>
        public SceneManager()
        {
            m_LoadedSceneAssetNames = new List<string>();
            m_LoadingSceneAssetNames = new List<string>();
            m_UnloadingSceneAssetNames = new List<string>();
            m_ResourceManager = null;
        }

        /// <summary>
        /// 获取游戏框架模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        internal override int Priority => 2;


        /// <summary>
        /// 场景管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        /// <summary>
        /// 关闭并清理场景管理器。
        /// </summary>
        internal override void Shutdown()
        {
            var loadedSceneAssetNames = m_LoadedSceneAssetNames.ToArray();
            // 退出游戏时已经在ResourcesComponent里调用了YooAssets.Destroy，无需再清理场景
            if (YooAssets.Initialized)
            {
                foreach (var loadedSceneAssetName in loadedSceneAssetNames)
                {
                    if (SceneIsUnloading(loadedSceneAssetName)) continue;
                    UnloadScene(loadedSceneAssetName).Forget();
                }
            }

            m_LoadedSceneAssetNames.Clear();
            m_LoadingSceneAssetNames.Clear();
            m_UnloadingSceneAssetNames.Clear();
        }

        /// <summary>
        /// 设置资源管理器。
        /// </summary>
        /// <param name="resourceManager">资源管理器。</param>
        public void SetResourceManager(IResourceManager resourceManager)
        {
            if (resourceManager == null) throw new GameFrameworkException("Resource manager is invalid.");

            m_ResourceManager = resourceManager;
        }

        /// <summary>
        /// 获取场景是否已加载。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>场景是否已加载。</returns>
        public bool SceneIsLoaded(string sceneAssetName)
        {
            if (string.IsNullOrEmpty(sceneAssetName)) throw new GameFrameworkException("Scene asset name is invalid.");

            return m_LoadedSceneAssetNames.Contains(sceneAssetName);
        }

        /// <summary>
        /// 获取已加载场景的资源名称。
        /// </summary>
        /// <returns>已加载场景的资源名称。</returns>
        public string[] GetLoadedSceneAssetNames()
        {
            return m_LoadedSceneAssetNames.ToArray();
        }

        /// <summary>
        /// 获取已加载场景的资源名称。
        /// </summary>
        /// <param name="results">已加载场景的资源名称。</param>
        public void GetLoadedSceneAssetNames(List<string> results)
        {
            if (results == null) throw new GameFrameworkException("Results is invalid.");

            results.Clear();
            results.AddRange(m_LoadedSceneAssetNames);
        }

        /// <summary>
        /// 获取场景是否正在加载。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>场景是否正在加载。</returns>
        public bool SceneIsLoading(string sceneAssetName)
        {
            if (string.IsNullOrEmpty(sceneAssetName)) throw new GameFrameworkException("Scene asset name is invalid.");

            return m_LoadingSceneAssetNames.Contains(sceneAssetName);
        }

        /// <summary>
        /// 获取正在加载场景的资源名称。
        /// </summary>
        /// <returns>正在加载场景的资源名称。</returns>
        public string[] GetLoadingSceneAssetNames()
        {
            return m_LoadingSceneAssetNames.ToArray();
        }

        /// <summary>
        /// 获取正在加载场景的资源名称。
        /// </summary>
        /// <param name="results">正在加载场景的资源名称。</param>
        public void GetLoadingSceneAssetNames(List<string> results)
        {
            if (results == null) throw new GameFrameworkException("Results is invalid.");

            results.Clear();
            results.AddRange(m_LoadingSceneAssetNames);
        }

        /// <summary>
        /// 获取场景是否正在卸载。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>场景是否正在卸载。</returns>
        public bool SceneIsUnloading(string sceneAssetName)
        {
            if (string.IsNullOrEmpty(sceneAssetName)) throw new GameFrameworkException("Scene asset name is invalid.");

            return m_UnloadingSceneAssetNames.Contains(sceneAssetName);
        }

        /// <summary>
        /// 获取正在卸载场景的资源名称。
        /// </summary>
        /// <returns>正在卸载场景的资源名称。</returns>
        public string[] GetUnloadingSceneAssetNames()
        {
            return m_UnloadingSceneAssetNames.ToArray();
        }

        /// <summary>
        /// 获取正在卸载场景的资源名称。
        /// </summary>
        /// <param name="results">正在卸载场景的资源名称。</param>
        public void GetUnloadingSceneAssetNames(List<string> results)
        {
            if (results == null) throw new GameFrameworkException("Results is invalid.");

            results.Clear();
            results.AddRange(m_UnloadingSceneAssetNames);
        }

        /// <summary>
        /// 检查场景资源是否存在。
        /// </summary>
        /// <param name="sceneAssetName">要检查场景资源的名称。</param>
        /// <returns>场景资源是否存在。</returns>
        public bool HasScene(string sceneAssetName)
        {
            return m_ResourceManager.HasAsset(sceneAssetName);
        }


        public async UniTask LoadSceneAsync(string sceneAssetName, string packageName = "",
            LoadSceneMode sceneMode = LoadSceneMode.Single, bool suspendLoad = false, uint priority = 100,
            Action<float> progress = null)
        {
            if (string.IsNullOrEmpty(sceneAssetName)) throw new GameFrameworkException("Scene asset name is invalid.");

            if (m_ResourceManager == null) throw new GameFrameworkException("You must set resource manager first.");

            if (SceneIsUnloading(sceneAssetName))
                throw new GameFrameworkException(Utility.Text.Format("Scene asset '{0}' is being unloaded.",
                    sceneAssetName));

            if (SceneIsLoading(sceneAssetName))
                throw new GameFrameworkException(Utility.Text.Format("Scene asset '{0}' is being loaded.",
                    sceneAssetName));

            if (SceneIsLoaded(sceneAssetName))
                throw new GameFrameworkException(Utility.Text.Format("Scene asset '{0}' is already loaded.",
                    sceneAssetName));


            m_LoadingSceneAssetNames.Add(sceneAssetName);
            var duration = Time.time;
            var handle = await m_ResourceManager.LoadSceneAsync(sceneAssetName, packageName, sceneMode, suspendLoad,
                priority,
                progress);
            if (handle.Status == EOperationStatus.Succeed)
            {
                m_LoadingSceneAssetNames.Remove(sceneAssetName);
                m_LoadedSceneAssetNames.Add(sceneAssetName);
            }
            else
            {
                var errorMessage = Utility.Text.Format("Can not load asset '{0}'.", handle.SceneName);
                m_LoadingSceneAssetNames.Remove(sceneAssetName);
                var appendErrorMessage =
                    Utility.Text.Format("Load scene failure, scene asset name '{0}', status '{1}', error message '{2}'.",
                        sceneAssetName, handle.Status, errorMessage);


                throw new GameFrameworkException(appendErrorMessage);
            }
        }


        /// <summary>
        /// 卸载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        public async UniTask UnloadScene(string sceneAssetName, Action<float> progress = null)
        {
            if (string.IsNullOrEmpty(sceneAssetName)) throw new GameFrameworkException("Scene asset name is invalid.");

            if (m_ResourceManager == null) throw new GameFrameworkException("You must set resource manager first.");

            if (SceneIsUnloading(sceneAssetName))
                throw new GameFrameworkException(Utility.Text.Format("Scene asset '{0}' is being unloaded.",
                    sceneAssetName));

            if (SceneIsLoading(sceneAssetName))
                throw new GameFrameworkException(Utility.Text.Format("Scene asset '{0}' is being loaded.",
                    sceneAssetName));

            if (!SceneIsLoaded(sceneAssetName))
                throw new GameFrameworkException(Utility.Text.Format("Scene asset '{0}' is not loaded yet.",
                    sceneAssetName));

            m_UnloadingSceneAssetNames.Add(sceneAssetName);
            var ok = await m_ResourceManager.UnloadScene(sceneAssetName, "", progress);
            if (ok)
            {
                m_UnloadingSceneAssetNames.Remove(sceneAssetName);
                m_LoadedSceneAssetNames.Remove(sceneAssetName);
            }
            else
            {
                m_UnloadingSceneAssetNames.Remove(sceneAssetName);
                throw new GameFrameworkException(Utility.Text.Format("Unload scene failure, scene asset name '{0}'.",
                    sceneAssetName));
            }
        }
    }
}