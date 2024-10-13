using UnityEngine;

namespace GFPro
{
    public enum CodeMode
    {
        Client       = 1,
        Server       = 2,
        ClientServer = 3
    }

    public enum BuildType
    {
        Debug,
        Release
    }

    [CreateAssetMenu(menuName = "GFPro/CreateGlobalConfig", fileName = "GlobalConfig", order = 0)]
    public class GlobalConfig : ScriptableObject
    {
        public CodeMode CodeMode;

        public bool EnableDll;

        public BuildType BuildType;

        public AppType AppType;
    }
}