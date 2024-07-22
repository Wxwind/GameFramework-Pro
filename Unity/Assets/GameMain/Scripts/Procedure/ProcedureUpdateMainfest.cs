//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityGameFramework.Runtime;
using YooAsset;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace StarForce
{
    public class ProcedureUpdateMainfest : ProcedureBase
    {
        public override bool UseNativeDialog => true;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            GameEntry.Event.Fire(this, PatchStatesChangeEventArgs.Create("初始化资源包！"));
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
            await operation;

            if (operation.Status != EOperationStatus.Succeed)
            {
                Debug.LogWarning(operation.Error);
                GameEntry.Event.Fire(this, PatchManifestUpdateFailedEventArgs.Create());
                return;
            }

            ChangeState<ProcedureCreatePackageDownloader>(procedureOwner);
        }
    }
}