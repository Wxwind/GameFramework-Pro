using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFramework.ObjectPool;
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

        /// <summary>
        /// 正在加载的资源列表。
        /// </summary>
        private readonly HashSet<string> m_assetLoadingList = new();

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

        private string GetCacheKey(string location, string packageName = "")
        {
            if (string.IsNullOrEmpty(packageName))
            {
                return location;
            }

            return $"{packageName}/{location}";
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
            if (string.IsNullOrEmpty(location))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }

            var assetObjectKey = GetCacheKey(location, packageName);

            // 防止重复获取同个资源的handle，类似ET的协程锁
            await TryWaitingLoading(assetObjectKey);

            var assetObject = m_AssetPool.Spawn(assetObjectKey);
            if (assetObject != null)
            {
                return assetObject.Target as T;
            }

            m_assetLoadingList.Add(assetObjectKey);
            var handle = GetAssetHandle<T>(location, packageName);
            await handle.ToUniTask(progress != null ? new Progress<float>(progress) : null);
            m_assetLoadingList.Remove(assetObjectKey);

            var ret = handle.AssetObject as T;

            assetObject = AssetObject.Create(assetObjectKey, handle.AssetObject, handle);
            m_AssetPool.Register(assetObject, true);

            return ret;
        }


        private async UniTask TryWaitingLoading(string assetObjectKey)
        {
            if (m_assetLoadingList.Contains(assetObjectKey))
            {
                await UniTask.WaitUntil(
                    () => !m_assetLoadingList.Contains(assetObjectKey));
            }
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

            var objectPoolManager = GameFrameworkEntry.GetModule<IObjectPoolManager>();
            SetObjectPoolManager(objectPoolManager);
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
            m_assetLoadingList.Clear();
            // FIXME: 场景管理模块在资源管理之后卸载，结果发现m_SceneDict里没有数据了所以卸载失败，场景加载逻辑最好也放在scenemanager里
            // m_SceneDict.Clear();
#if !UNITY_WEBGL
            YooAssets.Destroy();
#endif
        }
    }
}