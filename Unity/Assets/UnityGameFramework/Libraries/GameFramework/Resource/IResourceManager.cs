using System;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using YooAsset;
using Object = UnityEngine.Object;

namespace GameFramework.Resource
{
    /// <summary>
    ///     资源管理器接口。
    /// </summary>
    public interface IResourceManager
    {
        /// <summary>
        ///     获取资源只读区路径。
        /// </summary>
        string ReadOnlyPath { get; }

        /// <summary>
        ///     获取资源读写区路径。
        /// </summary>
        string ReadWritePath { get; }

        /// <summary>
        ///     设置资源只读区路径。
        /// </summary>
        /// <param name="readOnlyPath">资源只读区路径。</param>
        void SetReadOnlyPath(string readOnlyPath);

        /// <summary>
        ///     设置资源读写区路径。
        /// </summary>
        /// <param name="readWritePath">资源读写区路径。</param>
        void SetReadWritePath(string readWritePath);

        void Initialize();

        /// <summary>
        ///     检查资源是否存在。
        /// </summary>
        /// <param name="assetName">要检查资源的名称。</param>
        /// <returns>检查资源是否存在的结果。</returns>
        bool HasAsset(string assetName);

        UniTask<InitializationOperation> InitPackage(ResourceMode mode, string packageName);

        void LoadAssetAsync<T>(string location, string packageName = "",
            LoadAssetCallbacks loadAssetCallbacks = null, object userData = null)
            where T : Object;

        UniTask<T> LoadAssetAsync<T>(string location, string packageName = "", Action<float> progress = null)
            where T : Object;

        /// <summary>
        ///     异步加载场景。
        /// </summary>
        /// <param name="sceneName">要加载场景资源的名称</param>
        /// <param name="packageName">资源所在包名</param>
        /// <param name="sceneMode">场景加载模式</param>
        /// <param name="suspendLoad">加载完毕时是否挂起</param>
        /// <param name="priority">加载场景优先级</param>
        /// <param name="progress">场景进度更新回调。</param>
        UniTask<SceneHandle> LoadSceneAsync(string sceneName, string packageName = "",
            LoadSceneMode sceneMode = LoadSceneMode.Single, bool suspendLoad = false, uint priority = 100,
            Action<float> progress = null
        );

        /// <summary>
        ///     异步卸载场景。
        /// </summary>
        /// <param name="sceneAssetName">要卸载场景资源的名称。</param>
        /// <param name="packageName">资源所在包名</param>
        /// <param name="progress">场景进度更新回调。</param>
        UniTask<bool> UnloadScene(string sceneAssetName, string packageName = "", Action<float> progress = null);


        /// <summary>
        ///     卸载资源。
        /// </summary>
        /// <param name="location">要卸载的资源。</param>
        /// <param name="packageName">资源所在包名</param>
        void UnloadAsset(object location, string packageName = "DefaultPackage");

        void UnloadUnusedAssets();
    }
}