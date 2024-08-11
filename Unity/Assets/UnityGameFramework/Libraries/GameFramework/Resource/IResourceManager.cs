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

        public string HostServerIp { get; set; }

        string GameVersion { get; set; }

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

        UniTask<T> LoadAssetAsync<T>(string location, string packageName = "", Action<float> progress = null)
            where T : Object;


        /// <summary>
        ///     卸载资源。
        /// </summary>
        /// <param name="asset">要卸载的资源。</param>
        void UnloadAsset(Object asset);

        void UnloadUnusedAssets();
    }
}