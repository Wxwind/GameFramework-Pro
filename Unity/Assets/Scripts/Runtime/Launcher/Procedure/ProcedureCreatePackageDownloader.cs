using Cysharp.Threading.Tasks;
using GFPro;
using UnityEngine;
using YooAsset;
using ProcedureOwner = GFPro.IFsm<GFPro.Procedure.ProcedureComponent>;

namespace Game
{
    public class ProcedureCreatePackageDownloader : ProcedureBase
    {
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            UILaunchMgr.ShowTip(GameEntry.Localization.GetString("CreatePackageDownloader.Tips"));
            CreateDownloader(procedureOwner).Forget();
        }

        private async UniTaskVoid CreateDownloader(ProcedureOwner procedureOwner)
        {
            await new WaitForSecondsRealtime(0.5f);

            var packageName = (string)procedureOwner.GetData("PackageName").GetValue();
            var package = YooAssets.GetPackage(packageName);
            var downloadingMaxNum = 10;
            var failedTryAgain = 3;
            var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
            procedureOwner.SetData("Downloader", VarObject.FromObject(downloader));

            if (downloader.TotalDownloadCount == 0)
            {
                Log.Info("Not found any download files !");
                ChangeState<ProcedureUpdaterDone>(procedureOwner);
            }
            else
            {
                // 发现新更新文件后，挂起流程系统
                // TODO: 注意：开发者需要在下载前检测磁盘空间不足
                var totalDownloadCount = downloader.TotalDownloadCount;
                var totalDownloadBytes = downloader.TotalDownloadBytes;

                var sizeMB = totalDownloadBytes / 1048576f;
                sizeMB = Mathf.Clamp(sizeMB, 0.1f, float.MaxValue);
                var totalSizeMB = sizeMB.ToString("f1");
                UILaunchMgr.ShowMessageBox(GameEntry.Localization.GetString("CreatePackageDownloader.Message", totalDownloadCount, totalSizeMB),
                    () => { ChangeState<ProcedureDownloadPackageFiles>(procedureOwner); });
            }
        }
    }
}