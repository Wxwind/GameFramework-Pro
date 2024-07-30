namespace GameFramework.UI
{
    internal sealed partial class UIManager : GameFrameworkModule, IUIManager
    {
        private sealed class OpenUIFormInfo : IReference
        {
            private int m_SerialId;
            private UIGroup m_UIGroup;
            private bool m_PauseCoveredUIForm;


            public int SerialId
            {
                get { return m_SerialId; }
            }

            public UIGroup UIGroup
            {
                get { return m_UIGroup; }
            }

            public bool PauseCoveredUIForm
            {
                get { return m_PauseCoveredUIForm; }
            }


            public static OpenUIFormInfo Create(int serialId, UIGroup uiGroup, bool pauseCoveredUIForm)
            {
                OpenUIFormInfo openUIFormInfo = ReferencePool.Acquire<OpenUIFormInfo>();
                openUIFormInfo.m_SerialId = serialId;
                openUIFormInfo.m_UIGroup = uiGroup;
                openUIFormInfo.m_PauseCoveredUIForm = pauseCoveredUIForm;
                return openUIFormInfo;
            }

            public void Clear()
            {
                m_SerialId = 0;
                m_UIGroup = null;
                m_PauseCoveredUIForm = false;
            }
        }
    }
}