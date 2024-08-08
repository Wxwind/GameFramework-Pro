using System;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.Resource;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace Game
{
    public class ProcedureInitPackage : ProcedureBase
    {
        public override bool UseNativeDialog => true;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            UILaunchMgr.ShowTip(GameEntry.Localization.GetString("InitPackage.Tips"));
            InitPackage(procedureOwner).Forget();
        }

        private async UniTaskVoid InitPackage(ProcedureOwner procedureOwner)
        {
            try
            {
                string packageName = (string)procedureOwner.GetData("PackageName").GetValue();
                var resourceMode = GameEntry.Base.ResourceMode;
                var initializationOperation = await GameEntry.Resource.InitPackage(resourceMode, packageName);

                string version = initializationOperation.PackageVersion;
                Log.Info($"Init resource package version : {version}");
                GameEntry.Resource.ApplicableGameVersion = initializationOperation.PackageVersion;

                // 编辑器模式和单机模式直接跳过资源加载步骤

                // 编辑器模式。
                if (GameEntry.Base.ResourceMode == ResourceMode.EditorSimulateMode)
                {
                    Log.Info("EditorSimulateMode detected. Into ProcedurePreload.");
                    ChangeState<ProcedurePreload>(procedureOwner);
                }
                // 单机模式。
                else if (GameEntry.Base.ResourceMode == ResourceMode.OfflinePlayMode)
                {
                    Log.Info("OfflinePlayMode detected. Into ProcedurePreload.");
                    ChangeState<ProcedurePreload>(procedureOwner);
                }
                // 可更新模式。
                else if (GameEntry.Base.ResourceMode == ResourceMode.HostPlayMode ||
                         GameEntry.Base.ResourceMode == ResourceMode.WebPlayMode)
                {
                    // 打开启动UI。

                    Log.Info("Updatable resource mode detected. Into Into ProcedureUpdateVersion");
                    ChangeState<ProcedureUpdateVersion>(procedureOwner);
                }
                else
                {
                    Log.Error("UnKnow resource mode detected Please check???");
                }
            }
            catch (Exception e)
            {
                // 如果初始化失败弹出提示界面
                UILaunchMgr.ShowMessageBox(GameEntry.Localization.GetString("InitPackage.Failed"),
                    () => { ChangeState<ProcedureInitPackage>(procedureOwner); });
                throw new GameFrameworkException("Failed to initialize package.", e);
            }
        }
    }
}