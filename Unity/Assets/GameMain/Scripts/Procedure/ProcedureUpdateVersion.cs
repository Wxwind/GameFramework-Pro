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
    public class ProcedureUpdateVersion : ProcedureBase
    {
        public override bool UseNativeDialog => true;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);


            GameEntry.Event.Fire(this, PatchStatesChangeEventArgs.Create("获取最新的资源版本 !"));
            UpdatePackageVersion(procedureOwner).Forget();
        }

        private async UniTaskVoid UpdatePackageVersion(ProcedureOwner procedureOwner)
        {
            await new WaitForSecondsRealtime(0.5f);

            var packageName = (string)procedureOwner.GetData("PackageName").GetValue();
            var package = YooAssets.GetPackage(packageName);
            var operation = package.UpdatePackageVersionAsync();
            await operation;

            if (operation.Status != EOperationStatus.Succeed)
            {
                Log.Warning(operation.Error);
                GameEntry.Event.Fire(this, PackageVersionUpdateFailedEventArgs.Create());
            }
            else
            {
                procedureOwner.SetData<VarString>("PackageVersion", operation.PackageVersion);
                ChangeState<ProcedureUpdateMainfest>(procedureOwner);
            }
        }
    }
}