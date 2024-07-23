using Cysharp.Threading.Tasks;
using YooAsset;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace GameMain
{
    public class ProcedureDownloadPackageFiles : ProcedureBase
    {
        private ProcedureOwner ProcedureOwner;
        public override bool UseNativeDialog => true;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            ProcedureOwner = procedureOwner;

            UILaunchMgr.ShowTip("开始下载补丁文件！");
            BeginDownload(procedureOwner).Forget();
        }

        private async UniTaskVoid BeginDownload(ProcedureOwner procedureOwner)
        {
            var downloader = (ResourceDownloaderOperation)procedureOwner.GetData("Downloader").GetValue();
            downloader.OnDownloadErrorCallback = OnDownloadErrorCallback;
            downloader.OnDownloadProgressCallback = OnDownloadProgressCallback;
            await downloader.ToUniTask();

            // 检测下载结果
            if (downloader.Status != EOperationStatus.Succeed)
                return;

            ChangeState<ProcedureDownloadPackageOver>(procedureOwner);
        }

        private void OnDownloadErrorCallback(string fileName, string error)
        {
            UILaunchMgr.ShowMessageBox($"Failed to download file : {fileName}",
                () => { ChangeState<ProcedureDownloadPackageFiles>(ProcedureOwner); });
        }

        private void OnDownloadProgressCallback(int totalDownloadCount, int currentDownloadCount,
            long totalDownloadBytes, long currentDownloadBytes)
        {
            var value = (float)currentDownloadCount / totalDownloadCount;
            var currentSizeMB = (currentDownloadBytes / 1048576f).ToString("f1");
            var totalSizeMB = (totalDownloadBytes / 1048576f).ToString("f1");
            var text = $"{currentDownloadCount}/{totalDownloadCount} {currentSizeMB}MB/{totalSizeMB}MB";
            UILaunchMgr.ShowDownloadProgress(value, text);
        }
    }
}