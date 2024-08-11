using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityGameFramework.Runtime;
using YooAsset;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace Game
{
    public class ProcedureUpdateVersion : ProcedureBase
    {
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            UILaunchMgr.ShowTip(GameEntry.Localization.GetString("UpdateVersion.Tips"));
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Log.Warning("Not connected to the network");
                UILaunchMgr.ShowMessageBox(GameEntry.Localization.GetString("UpdateVersion.Error.Network"),
                    () => { ChangeState<ProcedureUpdateVersion>(procedureOwner); });
                return;
            }

            UpdatePackageVersion(procedureOwner).Forget();
        }

        private async UniTaskVoid UpdatePackageVersion(ProcedureOwner procedureOwner)
        {
            await new WaitForSecondsRealtime(0.5f);

            try
            {
                var packageName = (string)procedureOwner.GetData("PackageName").GetValue();
                var package = YooAssets.GetPackage(packageName);
                var operation = package.UpdatePackageVersionAsync();
                await operation.ToUniTask();
                Log.Info($"update resource package version: {operation.PackageVersion}");
                GameEntry.Resource.GameVersion = operation.PackageVersion;
                procedureOwner.SetData<VarString>("PackageVersion", operation.PackageVersion);
                ChangeState<ProcedureUpdateManifest>(procedureOwner);
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                UILaunchMgr.ShowMessageBox(GameEntry.Localization.GetString("UpdateVersion.Error.Network2"),
                    () => { ChangeState<ProcedureUpdateVersion>(procedureOwner); });
            }
        }
    }
}