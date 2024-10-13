using ProcedureOwner = GFPro.IFsm<GFPro.Procedure.ProcedureComponent>;

namespace Game
{
    public class ProcedureSplash : ProcedureBase
    {
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            // TODO: 这里可以播放一个 Splash 动画
            // ...

            UILaunchMgr.Initialize();
            ChangeState<ProcedureInitPackage>(procedureOwner);
        }
    }
}