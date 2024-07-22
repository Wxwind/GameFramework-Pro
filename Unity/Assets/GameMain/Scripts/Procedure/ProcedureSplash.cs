//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.IO;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Runtime;
using YooAsset;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace StarForce
{
    public class ProcedureSplash : ProcedureBase
    {
        public override bool UseNativeDialog => true;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            // TODO: 这里可以播放一个 Splash 动画
            // ...

            // TODO 编辑器模式和单机模式直接跳过资源加载步骤
            // if (GameEntry.Base.EditorResourceMode)
            // {
            //     // 编辑器模式
            //     Log.Info("Editor resource mode detected.");
            //     ChangeState<ProcedurePreload>(procedureOwner);
            // }
            // else if (GameEntry.Resource.ResourceMode == ResourceMode.Package)
            // {
            //     // 单机模式
            //     Log.Info("Package resource mode detected.");
            //     ChangeState<ProcedureInitResources>(procedureOwner);
            // }
            // else
            // {
            //     // 可更新模式
            //     Log.Info("Updatable resource mode detected.");
            //     ChangeState<ProcedureCheckVersion>(procedureOwner);
            // }
            GameEntry.Event.Fire(this, PatchStatesChangeEventArgs.Create("初始化资源包！"));
            InitPackage(procedureOwner).Forget();
        }

        private async UniTaskVoid InitPackage(ProcedureOwner procedureOwner)
        {
            var playMode = (EPlayMode)procedureOwner.GetData("PlayMode").GetValue();
            var packageName = (string)procedureOwner.GetData("PackageName").GetValue();
            var buildPipeline = (string)procedureOwner.GetData("BuildPipeline").GetValue();

            // 创建资源包裹类
            var package = YooAssets.TryGetPackage(packageName);
            if (package == null)
                package = YooAssets.CreatePackage(packageName);

            // 编辑器下的模拟模式
            InitializationOperation initializationOperation = null;
            if (playMode == EPlayMode.EditorSimulateMode)
            {
                var createParameters = new EditorSimulateModeParameters();
                createParameters.SimulateManifestFilePath =
                    EditorSimulateModeHelper.SimulateBuild(buildPipeline, packageName);
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // 单机运行模式
            if (playMode == EPlayMode.OfflinePlayMode)
            {
                var createParameters = new OfflinePlayModeParameters();
                createParameters.DecryptionServices = new FileStreamDecryption();
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // 联机运行模式
            if (playMode == EPlayMode.HostPlayMode)
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
            if (playMode == EPlayMode.WebPlayMode)
            {
                var defaultHostServer = GetHostServerURL();
                var fallbackHostServer = GetHostServerURL();
                var createParameters = new WebPlayModeParameters();
                createParameters.DecryptionServices = new FileStreamDecryption();
                createParameters.BuildinQueryServices = new GameQueryServices();
                createParameters.RemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                initializationOperation = package.InitializeAsync(createParameters);
            }

            await initializationOperation;

            // 如果初始化失败弹出提示界面
            if (initializationOperation.Status != EOperationStatus.Succeed)
            {
                Log.Warning($"{initializationOperation.Error}");
                GameEntry.Event.Fire(this, InitializeFailedEventArgs.Create());
            }
            else
            {
                var version = initializationOperation.PackageVersion;
                Log.Info($"Init resource package version : {version}");
                ChangeState<ProcedureUpdateVersion>(procedureOwner);
            }
        }

        /// <summary>
        ///     获取资源服务器地址
        /// </summary>
        private string GetHostServerURL()
        {
            //string hostServerIP = "http://10.0.2.2"; //安卓模拟器地址
            var hostServerIP = "http://127.0.0.1";
            var appVersion = "v1.0";

#if UNITY_EDITOR
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                return $"{hostServerIP}/CDN/Android/{appVersion}";
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
                return $"{hostServerIP}/CDN/IPhone/{appVersion}";
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL)
                return $"{hostServerIP}/CDN/WebGL/{appVersion}";
            return $"{hostServerIP}/CDN/PC/{appVersion}";
#else
        if (Application.platform == RuntimePlatform.Android)
            return $"{hostServerIP}/CDN/Android/{appVersion}";
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
            return $"{hostServerIP}/CDN/IPhone/{appVersion}";
        else if (Application.platform == RuntimePlatform.WebGLPlayer)
            return $"{hostServerIP}/CDN/WebGL/{appVersion}";
        else
            return $"{hostServerIP}/CDN/PC/{appVersion}";
#endif
        }

        /// <summary>
        ///     远端资源地址查询服务类
        /// </summary>
        private class RemoteServices : IRemoteServices
        {
            private readonly string _defaultHostServer;
            private readonly string _fallbackHostServer;

            public RemoteServices(string defaultHostServer, string fallbackHostServer)
            {
                _defaultHostServer = defaultHostServer;
                _fallbackHostServer = fallbackHostServer;
            }

            string IRemoteServices.GetRemoteMainURL(string fileName)
            {
                return $"{_defaultHostServer}/{fileName}";
            }

            string IRemoteServices.GetRemoteFallbackURL(string fileName)
            {
                return $"{_fallbackHostServer}/{fileName}";
            }
        }

        /// <summary>
        ///     资源文件流加载解密类
        /// </summary>
        private class FileStreamDecryption : IDecryptionServices
        {
            /// <summary>
            ///     同步方式获取解密的资源包对象
            ///     注意：加载流对象在资源包对象释放的时候会自动释放
            /// </summary>
            AssetBundle IDecryptionServices.LoadAssetBundle(DecryptFileInfo fileInfo, out Stream managedStream)
            {
                var bundleStream =
                    new BundleStream(fileInfo.FileLoadPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                managedStream = bundleStream;
                return AssetBundle.LoadFromStream(bundleStream, fileInfo.ConentCRC, GetManagedReadBufferSize());
            }

            /// <summary>
            ///     异步方式获取解密的资源包对象
            ///     注意：加载流对象在资源包对象释放的时候会自动释放
            /// </summary>
            AssetBundleCreateRequest IDecryptionServices.LoadAssetBundleAsync(DecryptFileInfo fileInfo,
                out Stream managedStream)
            {
                var bundleStream =
                    new BundleStream(fileInfo.FileLoadPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                managedStream = bundleStream;
                return AssetBundle.LoadFromStreamAsync(bundleStream, fileInfo.ConentCRC, GetManagedReadBufferSize());
            }

            private static uint GetManagedReadBufferSize()
            {
                return 1024;
            }
        }

        /// <summary>
        ///     资源文件偏移加载解密类
        /// </summary>
        private class FileOffsetDecryption : IDecryptionServices
        {
            /// <summary>
            ///     同步方式获取解密的资源包对象
            ///     注意：加载流对象在资源包对象释放的时候会自动释放
            /// </summary>
            AssetBundle IDecryptionServices.LoadAssetBundle(DecryptFileInfo fileInfo, out Stream managedStream)
            {
                managedStream = null;
                return AssetBundle.LoadFromFile(fileInfo.FileLoadPath, fileInfo.ConentCRC, GetFileOffset());
            }

            /// <summary>
            ///     异步方式获取解密的资源包对象
            ///     注意：加载流对象在资源包对象释放的时候会自动释放
            /// </summary>
            AssetBundleCreateRequest IDecryptionServices.LoadAssetBundleAsync(DecryptFileInfo fileInfo,
                out Stream managedStream)
            {
                managedStream = null;
                return AssetBundle.LoadFromFileAsync(fileInfo.FileLoadPath, fileInfo.ConentCRC, GetFileOffset());
            }

            private static ulong GetFileOffset()
            {
                return 32;
            }
        }
    }

    public class BundleStream : FileStream
    {
        public const byte KEY = 64;

        public BundleStream(string path, FileMode mode, FileAccess access, FileShare share) : base(path, mode, access,
            share)
        {
        }

        public BundleStream(string path, FileMode mode) : base(path, mode)
        {
        }

        public override int Read(byte[] array, int offset, int count)
        {
            var index = base.Read(array, offset, count);
            for (var i = 0; i < array.Length; i++) array[i] ^= KEY;
            return index;
        }
    }
}