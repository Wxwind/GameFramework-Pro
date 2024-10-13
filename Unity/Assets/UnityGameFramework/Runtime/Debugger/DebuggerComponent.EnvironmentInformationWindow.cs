using GFPro.Resource;
using UnityEngine;
using UnityEngine.Rendering;

namespace GFPro
{
    public sealed partial class DebuggerComponent : GameFrameworkComponent
    {
        private sealed class EnvironmentInformationWindow : ScrollableDebuggerWindowBase
        {
            private BaseComponent     m_BaseComponent;
            private ResourceComponent m_ResourceComponent;

            public override void Initialize(params object[] args)
            {
                m_BaseComponent = GameEntry.GetComponent<BaseComponent>();
                if (m_BaseComponent == null)
                {
                    Log.Fatal("Base component is invalid.");
                    return;
                }

                m_ResourceComponent = GameEntry.GetComponent<ResourceComponent>();
                if (m_ResourceComponent == null)
                {
                    Log.Fatal("Resource component is invalid.");
                }
            }

            protected override void OnDrawScrollableWindow()
            {
                GUILayout.Label("<b>Environment Information</b>");
                GUILayout.BeginVertical("box");
                {
                    DrawItem("Product Name", Application.productName);
                    DrawItem("Company Name", Application.companyName);
                    DrawItem("Game Identifier", Application.identifier);
                    DrawItem("Game Framework Version", Version.GameFrameworkVersion);
                    DrawItem("Game Version",
                        Utility.Text.Format("{0} ({1})", Version.GameVersion, Version.InternalGameVersion));
                    DrawItem("Resource Version",
                        m_BaseComponent.ResourceMode == ResourceMode.EditorSimulateMode
                            ? "Unavailable in editor resource mode"
                            : string.IsNullOrEmpty(m_ResourceComponent.GameVersion)
                                ? "Unknown"
                                : Utility.Text.Format("{0} ({1})", m_ResourceComponent.GameVersion,
                                    "Not Exists InternalResourceVersion"));
                    DrawItem("Application Version", Application.version);
                    DrawItem("Unity Version", Application.unityVersion);
                    DrawItem("Platform", Application.platform.ToString());
                    DrawItem("System Language", Application.systemLanguage.ToString());
                    DrawItem("Cloud Project Id", Application.cloudProjectId);
                    DrawItem("Build Guid", Application.buildGUID);
                    DrawItem("Target Frame Rate", Application.targetFrameRate.ToString());
                    DrawItem("Internet Reachability", Application.internetReachability.ToString());
                    DrawItem("Background Loading Priority", Application.backgroundLoadingPriority.ToString());
                    DrawItem("Is Playing", Application.isPlaying.ToString());
                    DrawItem("Splash Screen Is Finished", SplashScreen.isFinished.ToString());
                    DrawItem("Run In Background", Application.runInBackground.ToString());
                    DrawItem("Install Name", Application.installerName);
                    DrawItem("Install Mode", Application.installMode.ToString());
                    DrawItem("Sandbox Type", Application.sandboxType.ToString());
                    DrawItem("Is Mobile Platform", Application.isMobilePlatform.ToString());
                    DrawItem("Is Console Platform", Application.isConsolePlatform.ToString());
                    DrawItem("Is Editor", Application.isEditor.ToString());
                    DrawItem("Is Debug Build", Debug.isDebugBuild.ToString());
                    DrawItem("Is Focused", Application.isFocused.ToString());
                    DrawItem("Is Batch Mode", Application.isBatchMode.ToString());
                }
                GUILayout.EndVertical();
            }
        }
    }
}