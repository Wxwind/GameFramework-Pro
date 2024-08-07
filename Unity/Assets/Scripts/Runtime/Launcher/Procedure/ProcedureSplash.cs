using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace Game
{
    public class ProcedureSplash : ProcedureBase
    {
        public override bool UseNativeDialog => true;

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