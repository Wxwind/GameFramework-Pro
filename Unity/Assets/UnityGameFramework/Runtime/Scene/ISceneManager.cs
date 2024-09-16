using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityGameFramework.Resource;

namespace UnityGameFramework.Scene
{
    /// <summary>
    /// 场景管理器接口。
    /// </summary>
    public interface ISceneManager
    {
        /// <summary>
        /// 设置资源管理器。
        /// </summary>
        /// <param name="resourceComponent">资源管理器。</param>
        void SetResourceManager(IResourceComponent resourceComponent);

        /// <summary>
        /// 获取场景是否已加载。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>场景是否已加载。</returns>
        bool SceneIsLoaded(string sceneAssetName);

        /// <summary>
        /// 获取已加载场景的资源名称。
        /// </summary>
        /// <returns>已加载场景的资源名称。</returns>
        string[] GetLoadedSceneAssetNames();

        /// <summary>
        /// 获取已加载场景的资源名称。
        /// </summary>
        /// <param name="results">已加载场景的资源名称。</param>
        void GetLoadedSceneAssetNames(List<string> results);

        /// <summary>
        /// 获取场景是否正在加载。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>场景是否正在加载。</returns>
        bool SceneIsLoading(string sceneAssetName);

        /// <summary>
        /// 获取正在加载场景的资源名称。
        /// </summary>
        /// <returns>正在加载场景的资源名称。</returns>
        string[] GetLoadingSceneAssetNames();

        /// <summary>
        /// 获取正在加载场景的资源名称。
        /// </summary>
        /// <param name="results">正在加载场景的资源名称。</param>
        void GetLoadingSceneAssetNames(List<string> results);

        /// <summary>
        /// 获取场景是否正在卸载。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>场景是否正在卸载。</returns>
        bool SceneIsUnloading(string sceneAssetName);

        /// <summary>
        /// 获取正在卸载场景的资源名称。
        /// </summary>
        /// <returns>正在卸载场景的资源名称。</returns>
        string[] GetUnloadingSceneAssetNames();

        /// <summary>
        /// 获取正在卸载场景的资源名称。
        /// </summary>
        /// <param name="results">正在卸载场景的资源名称。</param>
        void GetUnloadingSceneAssetNames(List<string> results);

        /// <summary>
        /// 检查场景资源是否存在。
        /// </summary>
        /// <param name="sceneAssetName">要检查场景资源的名称。</param>
        /// <returns>场景资源是否存在。</returns>
        bool HasScene(string sceneAssetName);


        /// <summary>
        /// 加载场景 UniTask版。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="packageName">场景资源所在包名</param>
        /// <param name="priority">加载场景资源的优先级。</param>
        /// <param name="sceneMode">场景加载模式</param>
        /// <param name="suspendLoad">加载完成后是否挂起</param>
        /// <param name="progress">场景加载进度回调</param>
        UniTask LoadSceneAsync(string sceneAssetName, string packageName = "",
            LoadSceneMode sceneMode = LoadSceneMode.Single, bool suspendLoad = false, uint priority = 100,
            Action<float> progress = null);


        /// <summary>
        /// 卸载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="callbacks">场景加载回调。</param>
        UniTask UnloadScene(string sceneAssetName, Action<float> progress = null);

        void Shutdown();
    }
}