using Cysharp.Threading.Tasks;
using UnityGameFramework.Runtime;
using YooAsset;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace StarForce
{
    public class ProcedureClearPackageCache : ProcedureBase
    {
        public override bool UseNativeDialog => true;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            GameEntry.Event.Fire(this, PatchStatesChangeEventArgs.Create("清理未使用的缓存文件！"));
            ClearPackageCache(procedureOwner).Forget();
        }

        private async UniTaskVoid ClearPackageCache(ProcedureOwner procedureOwner)
        {
            var packageName = (string)procedureOwner.GetData("PackageName").GetValue();
            var package = YooAssets.GetPackage(packageName);
            var operation = package.ClearUnusedCacheFilesAsync();
            await operation;
            ChangeState<ProcedureUpdaterDone>(procedureOwner);
        }
    }
}