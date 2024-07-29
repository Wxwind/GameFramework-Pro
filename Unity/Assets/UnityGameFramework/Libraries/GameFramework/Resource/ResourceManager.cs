using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
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
        private readonly Dictionary<string, SceneHandle> m_SceneDict = new();

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


        public void LoadAssetAsync<T>(string location, string packageName = "",
            LoadAssetCallbacks loadAssetCallbacks = null, object userData = null)
            where T : Object
        {
            if (string.IsNullOrEmpty(location))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }

            var duration = Time.time;
            var handle = GetAssetHandle<T>(location, packageName);

            if (loadAssetCallbacks?.LoadAssetUpdateCallback != null)
            {
                InvokeProgress(location, handle, loadAssetCallbacks.LoadAssetUpdateCallback, userData).Forget();
            }


            handle.Completed += OnCompleted;
            return;

            void OnCompleted(AssetHandle handle)
            {
                loadAssetCallbacks?.LoadAssetSuccessCallback?.Invoke(location, handle.AssetObject, Time.time - duration,
                    userData);
                var asset = handle.AssetObject as T;
                if (asset == null)
                {
                    var errorMsg = Utility.Text.Format("Can not load asset '{0}' because :'{1}'.", location,
                        "asset is not exist");
                    loadAssetCallbacks?.LoadAssetFailureCallback?.Invoke(location, LoadResourceStatus.NotExist,
                        errorMsg, userData);
                }
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

        public void LoadSceneAsync(string location, string packageName = "",
            LoadSceneMode sceneMode = LoadSceneMode.Single, bool suspendLoad = false, uint priority = 100,
            LoadSceneCallbacks loadSceneCallbacks = null,
            object userData = null)
        {
            var duration = Time.time;
            var handle = GetSceneHandle(location, packageName, sceneMode, suspendLoad, priority);
            if (loadSceneCallbacks?.LoadSceneUpdateCallback != null)
            {
                InvokeProgress(location, handle, loadSceneCallbacks.LoadSceneUpdateCallback, userData).Forget();
            }

            handle.Completed += OnCompleted;
            return;

            void OnCompleted(SceneHandle h)
            {
                if (h.Status == EOperationStatus.Succeed)
                {
                    loadSceneCallbacks?.LoadSceneSuccessCallback?.Invoke(location, h, Time.time - duration,
                        userData);
                    m_SceneDict.Add(location, handle);
                }
                else
                {
                    var errorMessage = Utility.Text.Format("Can not load asset '{0}'.", location);
                    loadSceneCallbacks?.LoadSceneFailureCallback?.Invoke(location, LoadResourceStatus.NotReady,
                        errorMessage, userData);
                }
            }
        }

        public async UniTask<SceneHandle> LoadSceneAsync(string sceneAssetName, string packageName = "",
            LoadSceneMode sceneMode = LoadSceneMode.Single, bool suspendLoad = false, uint priority = 100,
            Action<float> progress = null
        )
        {
            var location = GetSceneAssetLocationKey(packageName, sceneAssetName);
            if (m_SceneDict.ContainsKey(location))
            {
                throw new GameFrameworkException($"scene asset {location} has already been loaded");
            }

            var handle = GetSceneHandle(location, packageName, sceneMode, suspendLoad, priority);
            await handle.ToUniTask(progress != null ? new Progress<float>(progress) : null);
            m_SceneDict.Add(location, handle);
            return handle;
        }

        private static string GetSceneAssetLocationKey(string sceneAssetName, string packageName = "")
        {
            return $"{packageName}/sceneAssetName";
        }

        public void UnloadScene(string sceneAssetName, string packageName = "", UnloadSceneCallbacks callbacks = null,
            object userData = null)
        {
            var location = GetSceneAssetLocationKey(packageName, sceneAssetName);
            if (m_SceneDict.TryGetValue(location, out var handle))
            {
                var unloadSceneOperation = handle.UnloadAsync();
                unloadSceneOperation.Completed += operation =>
                {
                    if (operation.Status == EOperationStatus.Failed)
                    {
                        callbacks?.UnloadSceneFailureCallback(sceneAssetName, userData);
                    }
                    else
                    {
                        callbacks?.UnloadSceneSuccessCallback(sceneAssetName, userData);
                        m_SceneDict.Remove(location);
                    }
                };
            }
            else
            {
                GameFrameworkLog.Error($"scene {location} not exist");
                callbacks?.UnloadSceneFailureCallback(sceneAssetName, userData);
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

        private async UniTaskVoid InvokeProgress(string location, SceneHandle assetHandle,
            LoadSceneUpdateCallback loadAssetUpdateCallback, object userData)
        {
            if (string.IsNullOrEmpty(location))
            {
                throw new GameFrameworkException("Asset Scene name is invalid.");
            }

            if (loadAssetUpdateCallback != null)
            {
                while (assetHandle is { IsValid: true, IsDone: false })
                {
                    await UniTask.Yield();

                    loadAssetUpdateCallback.Invoke(location, assetHandle.Progress, userData);
                }
            }
        }

        private async UniTaskVoid InvokeProgress(string location, AssetHandle assetHandle,
            LoadSceneUpdateCallback loadAssetUpdateCallback, object userData)
        {
            if (string.IsNullOrEmpty(location))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }

            if (loadAssetUpdateCallback != null)
            {
                while (assetHandle is { IsValid: true, IsDone: false })
                {
                    await UniTask.Yield();

                    loadAssetUpdateCallback.Invoke(location, assetHandle.Progress, userData);
                }
            }
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