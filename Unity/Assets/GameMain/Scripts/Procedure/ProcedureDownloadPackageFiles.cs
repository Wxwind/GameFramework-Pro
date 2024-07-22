using Cysharp.Threading.Tasks;
using UnityGameFramework.Runtime;
using YooAsset;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace StarForce
{
    public class ProcedureDownloadPackageFiles : ProcedureBase
    {
        public override bool UseNativeDialog => true;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            GameEntry.Event.Fire(this, PatchStatesChangeEventArgs.Create("开始下载补丁文件！"));
            BeginDownload(procedureOwner).Forget();
        }

        private async UniTaskVoid BeginDownload(ProcedureOwner procedureOwner)
        {
            var downloader = (ResourceDownloaderOperation)procedureOwner.GetData("Downloader").GetValue();
            downloader.OnDownloadErrorCallback = OnDownloadErrorCallback;
            downloader.OnDownloadProgressCallback = OnDownloadProgressCallback;
            await downloader;

            // 检测下载结果
            if (downloader.Status != EOperationStatus.Succeed)
                return;

            ChangeState<ProcedureDownloadPackageOver>(procedureOwner);
        }

        private void OnDownloadErrorCallback(string fileName, string error)
        {
            var msg = WebFileDownloadFailedEventArgs.Create(fileName, error);
            GameEntry.Event.Fire(this, msg);
        }

        private void OnDownloadProgressCallback(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)
        {
            var msg = DownloadProgressUpdateEventArgs.Create(totalDownloadCount, currentDownloadCount, totalDownloadBytes, currentDownloadBytes);
            GameEntry.Event.Fire(this, msg);
        }
    }
}