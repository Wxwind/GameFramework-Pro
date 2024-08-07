using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityGameFramework.Runtime;
using YooAsset;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace Game
{
    public class ProcedureCreatePackageDownloader : ProcedureBase
    {
        public override bool UseNativeDialog => true;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            UILaunchMgr.ShowTip("创建补丁下载器!");
            CreateDownloader(procedureOwner).Forget();
        }

        private async UniTaskVoid CreateDownloader(ProcedureOwner procedureOwner)
        {
            await new WaitForSecondsRealtime(0.5f);

            string packageName = (string)procedureOwner.GetData("PackageName").GetValue();
            var package = YooAssets.GetPackage(packageName);
            int downloadingMaxNum = 10;
            int failedTryAgain = 3;
            var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
            procedureOwner.SetData("Downloader", VarObject.FromObject(downloader));

            if (downloader.TotalDownloadCount == 0)
            {
                Debug.Log("Not found any download files !");
                ChangeState<ProcedureUpdaterDone>(procedureOwner);
            }
            else
            {
                // 发现新更新文件后，挂起流程系统
                // TODO: 注意：开发者需要在下载前检测磁盘空间不足
                int totalDownloadCount = downloader.TotalDownloadCount;
                long totalDownloadBytes = downloader.TotalDownloadBytes;


                float sizeMB = totalDownloadBytes / 1048576f;
                sizeMB = Mathf.Clamp(sizeMB, 0.1f, float.MaxValue);
                string totalSizeMB = sizeMB.ToString("f1");
                UILaunchMgr.ShowMessageBox(
                    $"Found update patch files, Total count {totalDownloadCount} Total size {totalSizeMB}MB",
                    () => { ChangeState<ProcedureDownloadPackageFiles>(procedureOwner); });
            }
        }
    }
}