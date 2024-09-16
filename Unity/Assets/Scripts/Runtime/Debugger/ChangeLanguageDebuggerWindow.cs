using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityGameFramework.Localization;
using UnityGameFramework.Runtime;

namespace Game
{
    public class ChangeLanguageDebuggerWindow : IDebuggerWindow
    {
        private bool    m_NeedRestart;
        private Vector2 m_ScrollPosition = Vector2.zero;

        public void Initialize(params object[] args)
        {
        }

        public void Shutdown()
        {
        }

        public void OnEnter()
        {
        }

        public void OnLeave()
        {
        }

        public void OnUpdate()
        {
            if (m_NeedRestart)
            {
                m_NeedRestart = false;
                UnityGameFramework.Runtime.GameEntry.Shutdown(ShutdownType.Restart);
            }
        }

        public void OnDraw()
        {
            m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition);
            {
                DrawSectionChangeLanguage();
            }
            GUILayout.EndScrollView();
        }

        private void DrawSectionChangeLanguage()
        {
            GUILayout.Label("<b>Change Language</b>");
            GUILayout.BeginHorizontal("box");
            {
                if (GUILayout.Button("Chinese Simplified", GUILayout.Height(30)))
                {
                    GameEntry.Localization.SetLanguage(Language.ChineseSimplified).Forget();
                    SaveLanguage();
                }

                if (GUILayout.Button("Chinese Traditional", GUILayout.Height(30)))
                {
                    GameEntry.Localization.SetLanguage(Language.ChineseTraditional).Forget();
                    SaveLanguage();
                }

                if (GUILayout.Button("English", GUILayout.Height(30)))
                {
                    GameEntry.Localization.SetLanguage(Language.English).Forget();
                    SaveLanguage();
                }
            }
            GUILayout.EndHorizontal();
        }

        private void SaveLanguage()
        {
            GameEntry.Setting.SetString(Constant.Setting.Language, GameEntry.Localization.Language.ToString());
            GameEntry.Setting.Save();
            m_NeedRestart = true;
        }
    }
}