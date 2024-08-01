using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using YooAsset;
using Object = UnityEngine.Object;

namespace GameFramework.Resource
{
    /// <summary>
    ///     资源管理器。
    /// </summary>
    internal sealed partial class ResourceManager : GameFrameworkModule, IResourceManager
    {
        private readonly Dictionary<string, ResourcePackage> m_PackagesDict = new();
        private readonly Dictionary<string, SceneHandle>     m_SceneDict    = new();

        public string ReadOnlyPath { get; set; }
        public string ReadWritePath { get; set; }


        public void SetReadOnlyPath(string readOnlyPath)
        {
            ReadOnlyPath = readOnlyPath;
        }

        public void SetReadWritePath(string readWritePath)
        {
            ReadWritePath = readWritePath;
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
            Action<float> progress = null) where T : Object
        {
            var handle = GetAssetHandle<T>(location, packageName);

            await handle.ToUniTask(progress != null ? new Progress<float>(progress) : null);
            return handle.AssetObject as T;
        }


        private SceneHandle GetSceneHandle(string location, string packageName = "",
            LoadSceneMode sceneMode = LoadSceneMode.Single, bool suspendLoad = false, uint priority = 100)
        {
            if (string.IsNullOrEmpty(packageName))
                return YooAssets.LoadSceneAsync(location, sceneMode, suspendLoad, priority);

            var package = YooAssets.GetPackage(packageName);
            return package.LoadSceneAsync(location, sceneMode, suspendLoad, priority);
        }

        public async UniTask<SceneHandle> LoadSceneAsync(string sceneName, string packageName = "",
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

        public async UniTask<bool> UnloadScene(string sceneName, string packageName = "", Action<float> progress = null)
        {
            if (m_SceneDict.TryGetValue(sceneName, out var handle))
            {
                var operation = handle.UnloadAsync();
                await operation.ToUniTask(progress != null ? new Progress<float>(progress) : null);

                if (operation.Status == EOperationStatus.Failed)
                {
                    return false;
                }
                else
                {
                    m_SceneDict.Remove(sceneName);
                    return true;
                }
            }
            else
            {
                GameFrameworkLog.Error($"UnloadScene error: scene {sceneName} not loaded");
                return false;
            }
        }

        public void UnloadAsset(object asset, string packageName = "DefaultPackage")
        {
            // TODO 使用对象池管理 asset handle
        }

        public void UnloadUnusedAssets()
        {
            foreach (var package in m_PackagesDict.Values)
            {
                if (package.InitializeStatus == EOperationStatus.Succeed)
                {
                    package.UnloadUnusedAssets();
                }
            }
        }

        public void Initialize()
        {
            if (!YooAssets.Initialized)
            {
                YooAssets.Initialize();
            }
        }


        public async UniTask<InitializationOperation> InitPackage(ResourceMode mode, string packageName)
        {
            if (m_PackagesDict.ContainsKey(packageName))
                throw new GameFrameworkException($"package '{packageName}' has already benn inited!");

            // 创建资源包裹类
            var package = YooAssets.TryGetPackage(packageName);
            if (package == null)
                package = YooAssets.CreatePackage(packageName);

            YooAssets.SetDefaultPackage(package);

            m_PackagesDict.Add(packageName, package);
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
                string defaultHostServer = GetHostServerURL();
                string fallbackHostServer = GetHostServerURL();
                var createParameters = new HostPlayModeParameters();
                createParameters.DecryptionServices = new FileStreamDecryption();
                createParameters.BuildinQueryServices = new GameQueryServices();
                createParameters.RemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // WebGL运行模式
            else if (mode == ResourceMode.WebPlayMode)
            {
                string defaultHostServer = GetHostServerURL();
                string fallbackHostServer = GetHostServerURL();
                var createParameters = new WebPlayModeParameters();
                createParameters.DecryptionServices = new FileStreamDecryption();
                createParameters.BuildinQueryServices = new GameQueryServices();
                createParameters.RemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                initializationOperation = package.InitializeAsync(createParameters);
            }
            else
            {
                throw new GameFrameworkException($"Unknown playmode {mode}");
            }

            await initializationOperation.ToUniTask();
            return initializationOperation;
        }


        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
            m_PackagesDict.Clear();
#if !UNITY_WEBGL
            YooAssets.Destroy();
#endif
        }
    }
}