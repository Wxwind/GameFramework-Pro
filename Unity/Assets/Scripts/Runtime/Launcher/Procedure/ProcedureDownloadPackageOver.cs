using ProcedureOwner = GFPro.IFsm<GFPro.Procedure.ProcedureComponent>;

namespace Game
{
    public class ProcedureDownloadPackageOver : ProcedureBase
    {
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            ChangeState<ProcedureClearPackageCache>(procedureOwner);
        }
    }
}