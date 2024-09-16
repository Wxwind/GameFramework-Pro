using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework;
using UnityGameFramework.Runtime;

namespace Game
{
    public class DialogForm : UGuiForm
    {
        [SerializeField] private Text m_TitleText;

        [SerializeField] private Text m_MessageText;

        [SerializeField] private GameObject[] m_ModeObjects;

        [SerializeField] private Text[] m_ConfirmTexts;

        [SerializeField] private Text[] m_CancelTexts;

        [SerializeField] private Text[] m_OtherTexts;

        private GameFrameworkAction<object> m_OnClickCancel;
        private GameFrameworkAction<object> m_OnClickConfirm;
        private GameFrameworkAction<object> m_OnClickOther;

        public int DialogMode { get; private set; } = 1;

        public bool PauseGame { get; private set; }

        public object UserData { get; private set; }

        public void OnConfirmButtonClick()
        {
            Close();

            if (m_OnClickConfirm != null) m_OnClickConfirm(UserData);
        }

        public void OnCancelButtonClick()
        {
            Close();

            if (m_OnClickCancel != null) m_OnClickCancel(UserData);
        }

        public void OnOtherButtonClick()
        {
            Close();

            if (m_OnClickOther != null) m_OnClickOther(UserData);
        }


        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            var dialogParams = (DialogParams)userData;
            if (dialogParams == null)
            {
                Log.Warning("DialogParams is invalid.");
                return;
            }

            DialogMode = dialogParams.Mode;
            RefreshDialogMode();

            m_TitleText.text = dialogParams.Title;
            m_MessageText.text = dialogParams.Message;

            PauseGame = dialogParams.PauseGame;
            RefreshPauseGame();

            UserData = dialogParams.UserData;

            RefreshConfirmText(dialogParams.ConfirmText);
            m_OnClickConfirm = dialogParams.OnClickConfirm;

            RefreshCancelText(dialogParams.CancelText);
            m_OnClickCancel = dialogParams.OnClickCancel;

            RefreshOtherText(dialogParams.OtherText);
            m_OnClickOther = dialogParams.OnClickOther;
        }


        protected override void OnClose(bool isShutdown)

        {
            if (PauseGame) GameEntry.Base.ResumeGame();

            DialogMode = 1;
            m_TitleText.text = string.Empty;
            m_MessageText.text = string.Empty;
            PauseGame = false;
            UserData = null;

            RefreshConfirmText(string.Empty);
            m_OnClickConfirm = null;

            RefreshCancelText(string.Empty);
            m_OnClickCancel = null;

            RefreshOtherText(string.Empty);
            m_OnClickOther = null;

            base.OnClose(isShutdown);
        }

        private void RefreshDialogMode()
        {
            for (var i = 1; i <= m_ModeObjects.Length; i++) m_ModeObjects[i - 1].SetActive(i == DialogMode);
        }

        private void RefreshPauseGame()
        {
            if (PauseGame) GameEntry.Base.PauseGame();
        }

        private void RefreshConfirmText(string confirmText)
        {
            if (string.IsNullOrEmpty(confirmText))
                confirmText = GameEntry.Localization.GetString("Dialog.ConfirmButton");

            for (var i = 0; i < m_ConfirmTexts.Length; i++) m_ConfirmTexts[i].text = confirmText;
        }

        private void RefreshCancelText(string cancelText)
        {
            if (string.IsNullOrEmpty(cancelText)) cancelText = GameEntry.Localization.GetString("Dialog.CancelButton");

            for (var i = 0; i < m_CancelTexts.Length; i++) m_CancelTexts[i].text = cancelText;
        }

        private void RefreshOtherText(string otherText)
        {
            if (string.IsNullOrEmpty(otherText)) otherText = GameEntry.Localization.GetString("Dialog.OtherButton");

            for (var i = 0; i < m_OtherTexts.Length; i++) m_OtherTexts[i].text = otherText;
        }
    }
}