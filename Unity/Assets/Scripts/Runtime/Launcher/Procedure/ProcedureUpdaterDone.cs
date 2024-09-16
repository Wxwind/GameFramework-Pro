using ProcedureOwner = UnityGameFramework.Fsm.IFsm<UnityGameFramework.Procedure.ProcedureComponent>;

namespace Game
{
    public class ProcedureUpdaterDone : ProcedureBase
    {
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            ChangeState<ProcedurePreload>(procedureOwner);
        }
    }
}