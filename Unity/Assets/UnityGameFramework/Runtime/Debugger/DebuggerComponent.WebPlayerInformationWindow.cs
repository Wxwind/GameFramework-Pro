using UnityEngine;

namespace GFPro
{
    public sealed partial class DebuggerComponent : GameFrameworkComponent
    {
        private sealed class WebPlayerInformationWindow : ScrollableDebuggerWindowBase
        {
            protected override void OnDrawScrollableWindow()
            {
                GUILayout.Label("<b>Web Player Information</b>");
                GUILayout.BeginVertical("box");
                {
                    DrawItem("Absolute URL", Application.absoluteURL);
                }
                GUILayout.EndVertical();
            }
        }
    }
}