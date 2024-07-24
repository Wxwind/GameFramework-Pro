using System;
using Cysharp.Threading.Tasks;
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


        /// <summary>
        ///     检查资源是否存在。
        /// </summary>
        /// <param name="assetName">要检查资源的名称。</param>
        /// <returns>检查资源是否存在的结果。</returns>
        bool HasAsset(string assetName);

        UniTask<InitializationOperation> InitPackage(ResourceMode mode, string packageName);

        UniTask<T> LoadAssetAsync<T>(string location, LoadAssetCallbacks loadAssetCallbacks, string packageName = "",
            IProgress<float> progress = null)
            where T : Object;

        UniTask<T> LoadAssetAsync<T>(string location, string packageName = "", IProgress<float> progress = null)
            where T : Object;


        /// <summary>
        ///     卸载资源。
        /// </summary>
        /// <param name="location">要卸载的资源。</param>
        void UnloadAsset(object location);

        void UnloadUnusedAssets();

        /// <summary>
        ///     异步加载场景。
        /// </summary>
        /// <param name="sceneAssetName">要加载场景资源的名称。</param>
        /// <param name="priority">加载场景优先级</param>
        /// <param name="loadSceneCallbacks">加载场景回调函数集。</param>
        /// <param name="userData">用户自定义数据</param>
        void LoadSceneAsync(string sceneAssetName, int priority, LoadSceneCallbacks loadSceneCallbacks, object userData);

        /// <summary>
        ///     异步卸载场景。
        /// </summary>
        /// <param name="sceneAssetName">要卸载场景资源的名称。</param>
        /// <param name="unloadSceneCallbacks">卸载场景回调函数集。</param>
        /// <param name="userData">用户自定义数据</param>
        void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData);
    }
}