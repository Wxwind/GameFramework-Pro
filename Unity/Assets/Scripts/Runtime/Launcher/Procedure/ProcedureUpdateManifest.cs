using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityGameFramework.Runtime;
using YooAsset;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace Game
{
    public class ProcedureUpdateManifest : ProcedureBase
    {
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            UILaunchMgr.ShowTip(GameEntry.Localization.GetString("UpdateManiFest.Tips"));
            UpdateManifest(procedureOwner).Forget();
        }


        private async UniTaskVoid UpdateManifest(ProcedureOwner procedureOwner)
        {
            await new WaitForSecondsRealtime(0.5f);

            try
            {
                string packageName = (string)procedureOwner.GetData("PackageName").GetValue();
                string packageVersion = (string)procedureOwner.GetData("PackageVersion").GetValue();
                var package = YooAssets.GetPackage(packageName);
                bool savePackageVersion = true;
                var operation = package.UpdatePackageManifestAsync(packageVersion, savePackageVersion);
                await operation.ToUniTask();

                operation.SavePackageVersion();
                ChangeState<ProcedureCreatePackageDownloader>(procedureOwner);
            }
            catch (Exception e)
            {
                Log.Error(e);
                UILaunchMgr.ShowMessageBox(GameEntry.Localization.GetString("UpdateManiFest.Error.Network"),
                    () => { ChangeState<ProcedureUpdateManifest>(procedureOwner); });
            }
        }
    }
}