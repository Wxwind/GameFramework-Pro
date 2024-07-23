using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace GameMain
{
    public class ProcedureUpdateManifest : ProcedureBase
    {
        public override bool UseNativeDialog => true;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            UILaunchMgr.ShowTip("更新资源清单!");
            UpdateManifest(procedureOwner).Forget();
        }


        private async UniTaskVoid UpdateManifest(ProcedureOwner procedureOwner)
        {
            await new WaitForSecondsRealtime(0.5f);

            var packageName = (string)procedureOwner.GetData("PackageName").GetValue();
            var packageVersion = (string)procedureOwner.GetData("PackageVersion").GetValue();
            var package = YooAssets.GetPackage(packageName);
            var savePackageVersion = true;
            var operation = package.UpdatePackageManifestAsync(packageVersion, savePackageVersion);
            await operation.ToUniTask();

            if (operation.Status != EOperationStatus.Succeed)
            {
                Debug.LogError(operation.Error);
                UILaunchMgr.ShowMessageBox("Failed to update patch manifest, please check the network status.",
                    () => { ChangeState<ProcedureUpdateManifest>(procedureOwner); });
                return;
            }

            operation.SavePackageVersion();
            ChangeState<ProcedureCreatePackageDownloader>(procedureOwner);
        }
    }
}