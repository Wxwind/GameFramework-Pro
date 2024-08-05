using Cysharp.Threading.Tasks;
using YooAsset;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace GameMain
{
    public class ProcedureClearPackageCache : ProcedureBase
    {
        public override bool UseNativeDialog => true;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            UILaunchMgr.ShowTip("清理未使用的缓存文件！");
            ClearPackageCache(procedureOwner).Forget();
        }

        private async UniTaskVoid ClearPackageCache(ProcedureOwner procedureOwner)
        {
            var packageName = (string)procedureOwner.GetData("PackageName").GetValue();
            var package = YooAssets.GetPackage(packageName);
            var operation = package.ClearUnusedCacheFilesAsync();
            await operation.ToUniTask();
            ChangeState<ProcedureUpdaterDone>(procedureOwner);
        }
    }
}