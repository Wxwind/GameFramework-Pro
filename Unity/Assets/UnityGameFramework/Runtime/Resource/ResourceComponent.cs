using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GFPro.ObjectPool;
using GFPro.Resource;
using UnityEngine;
using YooAsset;
using Object = UnityEngine.Object;

namespace GFPro
{
    /// <summary>
    ///     资源组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Resource")]
    public partial class ResourceComponent : GameFrameworkComponent, IResourceComponent
    {
        private const            string         DefaultPackage                  = "DefaultPackage";
        [SerializeField] private float          m_MinUnloadUnusedAssetsInterval = 60f;
        [SerializeField] private float          m_MaxUnloadUnusedAssetsInterval = 300f;
        private                  AsyncOperation m_AsyncOperation;
        private                  bool           m_ForceUnloadUnusedAssets;
        private                  float          m_LastUnloadUnusedAssetsOperationElapseSeconds;
        private                  bool           m_PerformGCCollect;
        private                  bool           m_PreorderUnloadUnusedAssets;

        private readonly Dictionary<string, ResourcePackage> m_PackagesDict = new();

        /// <summary>
        /// 正在加载的资源列表。
        /// </summary>
        private readonly HashSet<string> m_AssetLoadingList = new();

        public string HostServerIp { get; set; }

        public string GameVersion { get; set; }

        public string ReadOnlyPath { get; set; }
        public string ReadWritePath { get; set; }


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


        public void InitBuildInfo()
        {
            var builtinDataManager = GameEntry.GetComponent<BuiltinDataComponent>();
            if (builtinDataManager == null)
            {
                Log.Fatal("Builtin manager is invalid.");
                return;
            }

            var buildInfo = builtinDataManager.BuildInfo;
            if (YooAssets.Initialized)
            {
#if !UNITY_WEBGL
                YooAssets.Destroy();
#endif
            }

            YooAssets.Initialize();

            var objectPoolManager = GameEntry.GetComponent<ObjectPoolComponent>();
            InitAssetPool(objectPoolManager);
            HostServerIp = buildInfo.ServerHost;
            GameVersion = buildInfo.GameVersion;
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

                foreach (var package in m_PackagesDict.Values)
                {
                    if (package.InitializeStatus == EOperationStatus.Succeed)
                    {
                        package.UnloadUnusedAssets();
                    }
                }
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

        private void OnDestroy()
        {
            m_PackagesDict.Clear();
            m_AssetLoadingList.Clear();
#if !UNITY_WEBGL
            // 如果在这里销毁的话，则使用非编辑器模拟的资源加载模式时，因为场景是异步释放但是资源立马被清除了，所以退出游戏或者调用shutdown(restart)都会导致紫屏
            // YooAssets.Destroy();
#endif
        }

        public void SetReadOnlyPath(string readOnlyPath)
        {
            ReadOnlyPath = readOnlyPath;
        }

        public void SetReadWritePath(string readWritePath)
        {
            ReadWritePath = readWritePath;
        }


        public async UniTask<InitializationOperation> InitPackage(ResourceMode mode, string packageName = DefaultPackage)
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
                // TODO 从全局配置读入host server和版本号信息
                var defaultHostServer = GetHostServerURL(HostServerIp, GameVersion);
                var fallbackHostServer = GetHostServerURL(HostServerIp, GameVersion);
                var createParameters = new HostPlayModeParameters();
                createParameters.DecryptionServices = new FileStreamDecryption();
                createParameters.BuildinQueryServices = new GameQueryServices();
                createParameters.RemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // WebGL运行模式
            else if (mode == ResourceMode.WebPlayMode)
            {
                // TODO 从全局配置读入host server和版本号信息
                var defaultHostServer = GetHostServerURL(HostServerIp, GameVersion);
                var fallbackHostServer = GetHostServerURL(HostServerIp, GameVersion);
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


        public async UniTask<T> LoadAssetAsync<T>(string location, string packageName = DefaultPackage, Action<float> progress = null)
            where T : Object
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

            m_AssetLoadingList.Add(assetObjectKey);
            var handle = GetAssetHandle<T>(location, packageName);
            await handle.ToUniTask(progress != null ? new Progress<float>(progress) : null);
            m_AssetLoadingList.Remove(assetObjectKey);

            var ret = handle.AssetObject as T;

            assetObject = AssetObject.Create(assetObjectKey, handle.AssetObject, handle);
            m_AssetPool.Register(assetObject, true);

            return ret;
        }

        private async UniTask TryWaitingLoading(string assetObjectKey)
        {
            if (m_AssetLoadingList.Contains(assetObjectKey))
            {
                await UniTask.WaitUntil(
                    () => !m_AssetLoadingList.Contains(assetObjectKey));
            }
        }

        private AssetHandle GetAssetHandle<T>(string location, string packageName) where T : Object
        {
            if (string.IsNullOrEmpty(packageName)) return YooAssets.LoadAssetAsync<T>(location);

            var package = YooAssets.GetPackage(packageName);
            return package.LoadAssetAsync<T>(location);
        }


        private string GetCacheKey(string location, string packageName = "")
        {
            if (string.IsNullOrEmpty(packageName))
            {
                return location;
            }

            return $"{packageName}/{location}";
        }

        public bool HasAsset(string assetName)
        {
            return YooAssets.CheckLocationValid(assetName);
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