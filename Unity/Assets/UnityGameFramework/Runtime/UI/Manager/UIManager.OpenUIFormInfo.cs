namespace UnityGameFramework.UI
{
    internal sealed partial class UIManager
    {
        private sealed class OpenUIFormInfo : IReference
        {
            private int     m_SerialId;
            private UIGroup m_UIGroup;
            private bool    m_PauseCoveredUIForm;


            public int SerialId => m_SerialId;

            public UIGroup UIGroup => m_UIGroup;

            public bool PauseCoveredUIForm => m_PauseCoveredUIForm;


            public static OpenUIFormInfo Create(int serialId, UIGroup uiGroup, bool pauseCoveredUIForm)
            {
                var openUIFormInfo = ReferencePool.Acquire<OpenUIFormInfo>();
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