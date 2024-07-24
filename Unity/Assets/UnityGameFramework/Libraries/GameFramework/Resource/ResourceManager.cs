using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;
using Object = UnityEngine.Object;

namespace GameFramework.Resource
{
    /// <summary>
    ///     资源管理器。
    /// </summary>
    internal sealed partial class ResourceManager : GameFrameworkModule, IResourceManager
    {
        private readonly Dictionary<string, ResourcePackage> m_PackagesDic = new();

        public ResourceMode playMode;
        public string ReadOnlyPath { get; }
        public string ReadWritePath { get; }


        public void SetReadOnlyPath(string readOnlyPath)
        {
            throw new NotImplementedException();
        }

        public void SetReadWritePath(string readWritePath)
        {
            throw new NotImplementedException();
        }


        public bool HasAsset(string assetName)
        {
            return YooAssets.CheckLocationValid(assetName);
        }

        private AssetHandle GetAssetHandle<T>(string location, string packageName) where T : Object
        {
            if (string.IsNullOrEmpty(packageName)) return YooAssets.LoadAssetAsync<T>(location);

            var package = YooAssets.GetPackage(packageName);
            return package.LoadAssetAsync<T>(location);
        }

        public async UniTask<T> LoadAssetAsync<T>(string location, string packageName = "",
            IProgress<float> progress = null) where T : Object
        {
            var handle = GetAssetHandle<T>(location, packageName);
            await handle.ToUniTask(progress);
            return handle.AssetObject as T;
        }

        public async UniTask<T> LoadAssetAsync<T>(string location, string packageName = "", LoadAssetCallbacks loadAssetCallbacks = null, object userData = null)
            where T : Object
        {
            if (string.IsNullOrEmpty(location))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }

            var duration = Time.time;
            var handle = GetAssetHandle<T>(location, packageName);
            await handle.ToUniTask(loadAssetCallbacks.Progress);
            loadAssetCallbacks.LoadAssetSuccessCallback?.Invoke(location, handle.AssetObject, Time.time - duration, userData);
            var asset = handle.AssetObject as T;
            if (asset == null)
            {
                var errorMsg =
                    Utility.Text.Format("Can not load asset '{0}' because :'{1}'.", location, "asset is not exist");
                loadAssetCallbacks.LoadAssetFailureCallback?.Invoke(location, LoadResourceStatus.NotExist, errorMsg, userData);
            }

            return asset;
        }

        public async UniTask<T> LoadSceneAsync<T>(string location, string packageName = "",
            IProgress<float> progress = null) where T : Object
        {
            var handle = GetAssetHandle<T>(location, packageName);
            await handle.ToUniTask(progress);
            return handle.AssetObject as T;
        }

        public void UnloadAsset(object asset)
        {
            var package = YooAssets.GetPackage("DefaultPackage");
            package.TryUnloadUnusedAsset("Assets/GameRes/Panel/login.prefab");
        }

        public void UnloadUnusedAssets()
        {
            foreach (var package in m_PackagesDic.Values)
            {
                if (package.InitializeStatus == EOperationStatus.Succeed)
                {
                    package.UnloadUnusedAssets();
                }
            }
        }

        public void LoadSceneAsync(string sceneAssetName, int priority, LoadSceneCallbacks loadSceneCallbacks,
            object userData)
        {
            throw new NotImplementedException();
        }

        public void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData)
        {
            throw new NotImplementedException();
        }


        public async UniTask<InitializationOperation> InitPackage(ResourceMode mode, string packageName)
        {
            if (m_PackagesDic.ContainsKey(packageName))
                throw new GameFrameworkException($"package '{packageName}' has already benn inited!");

            // 创建资源包裹类
            var package = YooAssets.TryGetPackage(packageName);
            if (package == null)
                package = YooAssets.CreatePackage(packageName);

            m_PackagesDic.Add(packageName, package);
            // 编辑器下的模拟模式
            InitializationOperation initializationOperation = null;
            if (mode == ResourceMode.EditorSimulateMode)
            {
                var createParameters = new EditorSimulateModeParameters();
                createParameters.SimulateManifestFilePath =
                    EditorSimulateModeHelper.SimulateBuild(EDefaultBuildPipeline.BuiltinBuildPipeline, packageName);
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // 单机运行模式
            else if (mode == ResourceMode.OfflinePlayMode)
            {
                var createParameters = new OfflinePlayModeParameters();
                createParameters.DecryptionServices = new FileStreamDecryption();
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // 联机运行模式
            else if (mode == ResourceMode.HostPlayMode)
            {
                var defaultHostServer = GetHostServerURL();
                var fallbackHostServer = GetHostServerURL();
                var createParameters = new HostPlayModeParameters();
                createParameters.DecryptionServices = new FileStreamDecryption();
                createParameters.BuildinQueryServices = new GameQueryServices();
                createParameters.RemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // WebGL运行模式
            else if (mode == ResourceMode.WebPlayMode)
            {
                var defaultHostServer = GetHostServerURL();
                var fallbackHostServer = GetHostServerURL();
                var createParameters = new WebPlayModeParameters();
                createParameters.DecryptionServices = new FileStreamDecryption();
                createParameters.BuildinQueryServices = new GameQueryServices();
                createParameters.RemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                initializationOperation = package.InitializeAsync(createParameters);
            }
            else
            {
                throw new GameFrameworkException($"Unknown playmode {playMode}");
            }

            await initializationOperation.ToUniTask();
            return initializationOperation;
        }


        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
            m_PackagesDic.Clear();
        }
    }
}