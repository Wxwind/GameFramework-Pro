using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityGameFramework.Runtime;
using YooAsset;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace GameMain
{
    public class ProcedureUpdateVersion : ProcedureBase
    {
        public override bool UseNativeDialog => true;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            UILaunchMgr.ShowTip("获取最新的资源版本中...");
            if (Application.internetReachability== NetworkReachability.NotReachable)
            {
                Log.Warning("Not connected to the network");
                UILaunchMgr.ShowTip("Check the network please");
                UILaunchMgr.ShowMessageBox("Current network is inaccessible, please check and retry again.", () =>
                {
                    ChangeState<ProcedureUpdateVersion>(procedureOwner);
                });
                return;
            }
            
            UpdatePackageVersion(procedureOwner).Forget();
        }

        private async UniTaskVoid UpdatePackageVersion(ProcedureOwner procedureOwner)
        {
            await new WaitForSecondsRealtime(0.5f);

            var packageName = (string)procedureOwner.GetData("PackageName").GetValue();
            var package = YooAssets.GetPackage(packageName);
            var operation = package.UpdatePackageVersionAsync();
            await operation.ToUniTask();

            if (operation.Status != EOperationStatus.Succeed)
            {
                Log.Warning(operation.Error);
                UILaunchMgr.ShowMessageBox("Failed to update static version, please check the network status.",
                    () => { ChangeState<ProcedureUpdateVersion>(procedureOwner); });
            }
            else
            {
                procedureOwner.SetData<VarString>("PackageVersion", operation.PackageVersion);
                ChangeState<ProcedureUpdateManifest>(procedureOwner);
            }
        }
    }
}