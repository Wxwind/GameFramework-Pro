using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GFPro.Resource;
using UnityEngine.SceneManagement;
using YooAsset;

namespace GFPro
{
    /// <summary>
    /// 场景管理器。
    /// </summary>
    internal sealed class SceneManager : ISceneManager
    {
        private readonly List<string>       m_LoadedSceneAssetNames;
        private readonly List<string>       m_LoadingSceneAssetNames;
        private readonly List<string>       m_UnloadingSceneAssetNames;
        private          IResourceComponent m_ResourceComponent;

        private readonly Dictionary<string, SceneHandle> m_SceneDict = new();


        /// <summary>
        /// 初始化场景管理器的新实例。
        /// </summary>
        public SceneManager()
        {
            m_LoadedSceneAssetNames = new List<string>();
            m_LoadingSceneAssetNames = new List<string>();
            m_UnloadingSceneAssetNames = new List<string>();
            m_ResourceComponent = null;
        }


        /// <summary>
        /// 关闭并清理场景管理器。
        /// </summary>
        public void Shutdown()
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
        /// <param name="resourceComponent">资源管理器。</param>
        public void SetResourceManager(IResourceComponent resourceComponent)
        {
            if (resourceComponent == null) throw new GameFrameworkException("Resource manager is invalid.");

            m_ResourceComponent = resourceComponent;
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
            return m_ResourceComponent.HasAsset(sceneAssetName);
        }


        public async UniTask LoadSceneAsync(string sceneAssetName, string packageName = "",
            LoadSceneMode sceneMode = LoadSceneMode.Single, bool suspendLoad = false, uint priority = 100,
            Action<float> progress = null)
        {
            if (string.IsNullOrEmpty(sceneAssetName)) throw new GameFrameworkException("Scene asset name is invalid.");

            if (m_ResourceComponent == null) throw new GameFrameworkException("You must set resource manager first.");

            if (SceneIsUnloading(sceneAssetName))
                throw new GameFrameworkException($"Scene asset '{sceneAssetName}' is being unloaded.");

            if (SceneIsLoading(sceneAssetName))
                throw new GameFrameworkException($"Scene asset '{sceneAssetName}' is being loaded.");

            if (SceneIsLoaded(sceneAssetName))
                throw new GameFrameworkException($"Scene asset '{sceneAssetName}' is already loaded.");


            m_LoadingSceneAssetNames.Add(sceneAssetName);
            var handle = await LoadSceneAsyncInternal(sceneAssetName, packageName, sceneMode, suspendLoad,
                priority,
                progress);
            if (handle.Status == EOperationStatus.Succeed)
            {
                m_LoadingSceneAssetNames.Remove(sceneAssetName);
                m_LoadedSceneAssetNames.Add(sceneAssetName);
            }
            else
            {
                var errorMessage = $"Can not load asset '{handle.SceneName}'.";
                m_LoadingSceneAssetNames.Remove(sceneAssetName);
                var appendErrorMessage = $"Load scene failure, scene asset name '{sceneAssetName}', status '{handle.Status}', error message '{errorMessage}'.";


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

            if (m_ResourceComponent == null) throw new GameFrameworkException("You must set resource manager first.");

            if (SceneIsUnloading(sceneAssetName))
                throw new GameFrameworkException($"Scene asset '{sceneAssetName}' is being unloaded.");

            if (SceneIsLoading(sceneAssetName))
                throw new GameFrameworkException($"Scene asset '{sceneAssetName}' is being loaded.");

            if (SceneIsLoaded(sceneAssetName))
                throw new GameFrameworkException($"Scene asset '{sceneAssetName}' is already loaded.");


            try
            {
                m_UnloadingSceneAssetNames.Add(sceneAssetName);
                await UnloadSceneInternal(sceneAssetName, progress);

                m_UnloadingSceneAssetNames.Remove(sceneAssetName);
                m_LoadedSceneAssetNames.Remove(sceneAssetName);
            }
            catch (Exception e)
            {
                m_UnloadingSceneAssetNames.Remove(sceneAssetName);
                throw new GameFrameworkException($"Unload scene failure, scene asset name '{sceneAssetName}' reason: '{e.Message}'.");
            }
        }


        private SceneHandle GetSceneHandle(string location, string packageName = "",
            LoadSceneMode sceneMode = LoadSceneMode.Single, bool suspendLoad = false, uint priority = 100)
        {
            if (string.IsNullOrEmpty(packageName))
                return YooAssets.LoadSceneAsync(location, sceneMode, suspendLoad, priority);

            var package = YooAssets.GetPackage(packageName);
            return package.LoadSceneAsync(location, sceneMode, suspendLoad, priority);
        }

        /// <summary>
        ///     异步加载场景。
        /// </summary>
        /// <param name="sceneName">要加载场景资源的名称</param>
        /// <param name="packageName">资源所在包名</param>
        /// <param name="sceneMode">场景加载模式</param>
        /// <param name="suspendLoad">加载完毕时是否挂起</param>
        /// <param name="priority">加载场景优先级</param>
        /// <param name="progress">场景进度更新回调。</param>
        private async UniTask<SceneHandle> LoadSceneAsyncInternal(string sceneName, string packageName = "",
            LoadSceneMode sceneMode = LoadSceneMode.Single, bool suspendLoad = false, uint priority = 100,
            Action<float> progress = null
        )
        {
            if (m_SceneDict.ContainsKey(sceneName))
            {
                throw new GameFrameworkException($"scene asset {sceneName} has already been loaded");
            }

            var handle = GetSceneHandle(sceneName, packageName, sceneMode, suspendLoad, priority);
            await handle.ToUniTask(progress != null ? new Progress<float>(progress) : null);
            m_SceneDict.Add(sceneName, handle);
            return handle;
        }


        /// <summary>
        ///     异步卸载场景。
        /// </summary>
        /// <param name="sceneName">要卸载场景资源的名称。</param>
        /// <param name="progress">场景进度更新回调。</param>
        private async UniTask UnloadSceneInternal(string sceneName, Action<float> progress = null)
        {
            if (m_SceneDict.TryGetValue(sceneName, out var handle))
            {
                var operation = handle.UnloadAsync();
                await operation.ToUniTask(progress != null ? new Progress<float>(progress) : null);
                m_SceneDict.Remove(sceneName);
            }
            else
            {
                throw new GameFrameworkException($"UnloadScene error: scene {sceneName} not loaded");
            }
        }
    }
}