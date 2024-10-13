using System;
using Cysharp.Threading.Tasks;
using GFPro;
using YooAsset;
using ProcedureOwner = GFPro.IFsm<GFPro.Procedure.ProcedureComponent>;

namespace Game
{
    public class ProcedureDownloadPackageFiles : ProcedureBase
    {
        private ProcedureOwner ProcedureOwner;


        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            ProcedureOwner = procedureOwner;

            UILaunchMgr.ShowTip(GameEntry.Localization.GetString("DownloadPackageFiles.Tips"));
            BeginDownload(procedureOwner).Forget();
        }

        private async UniTaskVoid BeginDownload(ProcedureOwner procedureOwner)
        {
            try
            {
                var downloader = (ResourceDownloaderOperation)procedureOwner.GetData("Downloader").GetValue();
                downloader.OnDownloadProgressCallback = OnDownloadProgressCallback;
                await downloader.ToUniTask();
                ChangeState<ProcedureDownloadPackageOver>(procedureOwner);
            }
            catch (Exception e)
            {
                Log.Error($"Failed to download file, error: {e}");
                UILaunchMgr.ShowMessageBox(GameEntry.Localization.GetString("DownloadPackageFiles.Error.Network"),
                    () => { ChangeState<ProcedureDownloadPackageFiles>(ProcedureOwner); });
            }
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