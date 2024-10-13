using UnityEngine;

namespace GFPro
{
    public sealed partial class DebuggerComponent : GameFrameworkComponent
    {
        private sealed class SceneInformationWindow : ScrollableDebuggerWindowBase
        {
            protected override void OnDrawScrollableWindow()
            {
                GUILayout.Label("<b>Scene Information</b>");
                GUILayout.BeginVertical("box");
                {
                    DrawItem("Scene Count", UnityEngine.SceneManagement.SceneManager.sceneCount.ToString());
                    DrawItem("Scene Count In Build Settings", UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings.ToString());

                    var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                    DrawItem("Active Scene Handle", activeScene.handle.ToString());
                    DrawItem("Active Scene Name", activeScene.name);
                    DrawItem("Active Scene Path", activeScene.path);
                    DrawItem("Active Scene Build Index", activeScene.buildIndex.ToString());
                    DrawItem("Active Scene Is Dirty", activeScene.isDirty.ToString());
                    DrawItem("Active Scene Is Loaded", activeScene.isLoaded.ToString());
                    DrawItem("Active Scene Is Valid", activeScene.IsValid().ToString());
                    DrawItem("Active Scene Root Count", activeScene.rootCount.ToString());
                    DrawItem("Active Scene Is Sub Scene", activeScene.isSubScene.ToString());
                }
                GUILayout.EndVertical();
            }
        }
    }
}