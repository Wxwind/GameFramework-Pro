using Cysharp.Threading.Tasks;
using YooAsset;
using ProcedureOwner = GFPro.IFsm<GFPro.Procedure.ProcedureComponent>;

namespace Game
{
    public class ProcedureClearPackageCache : ProcedureBase
    {
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            UILaunchMgr.ShowTip(GameEntry.Localization.GetString("ClearPackageCache.Tips"));
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