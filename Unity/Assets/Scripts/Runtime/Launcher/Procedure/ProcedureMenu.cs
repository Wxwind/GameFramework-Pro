﻿using Cysharp.Threading.Tasks;
using GFPro;
using ProcedureOwner = GFPro.Fsm.IFsm<GFPro.Procedure.ProcedureComponent>;

namespace Game
{
    public class ProcedureMenu : ProcedureBase
    {
        private MenuForm m_MenuForm;
        private bool     m_StartGame;


        public void StartGame()
        {
            m_StartGame = true;
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            Log.Info("change into ProcedureMenu");

            m_StartGame = false;
            OpenMenuForm().Forget();
        }

        private async UniTaskVoid OpenMenuForm()
        {
            var form = await GameEntry.UI.OpenUIForm(UIFormId.MenuForm, this) as UIForm;
            if (form == null) return;
            m_MenuForm = (MenuForm)form.Logic;
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);

            if (m_MenuForm != null)
            {
                m_MenuForm.Close(isShutdown);
                m_MenuForm = null;
            }
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (m_StartGame)
            {
                procedureOwner.SetData<VarInt32>("NextSceneId", GameEntry.Config.GetInt("Scene.Main"));
                procedureOwner.SetData<VarByte>("GameMode", (byte)GameMode.Survival);
                ChangeState<ProcedureChangeScene>(procedureOwner);
            }
        }
    }
}