using System;
using Cysharp.Threading.Tasks;
using GFPro;
using UnityEngine;
using YooAsset;
using ProcedureOwner = GFPro.Fsm.IFsm<GFPro.Procedure.ProcedureComponent>;

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
                var packageName = (string)procedureOwner.GetData("PackageName").GetValue();
                var packageVersion = (string)procedureOwner.GetData("PackageVersion").GetValue();
                var package = YooAssets.GetPackage(packageName);
                var savePackageVersion = true;
                var operation = package.UpdatePackageManifestAsync(packageVersion, savePackageVersion);
                await operation.ToUniTask();

                operation.SavePackageVersion();
                ChangeState<ProcedureCreatePackageDownloader>(procedureOwner);
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                UILaunchMgr.ShowMessageBox(GameEntry.Localization.GetString("UpdateManiFest.Error.Network"),
                    () => { ChangeState<ProcedureUpdateManifest>(procedureOwner); });
            }
        }
    }
}