using UnityEngine;
using UnityGameFramework.BuiltinData;

namespace UnityGameFramework.Runtime
{
    public class BuiltinDataComponent : GameFrameworkComponent, IBuiltinDataComponent
    {
        [SerializeField] private TextAsset m_BuildInfoTextAsset;

        public BuildInfo BuildInfo { get; set; }


        public void InitBuildInfo()
        {
            if (m_BuildInfoTextAsset == null || string.IsNullOrEmpty(m_BuildInfoTextAsset.text))
            {
                Log.Info("Build info can not be found or empty.");
                return;
            }

            BuildInfo = Utility.Json.ToObject<BuildInfo>(m_BuildInfoTextAsset.text);
            if (BuildInfo == null) Log.Warning("Parse build info failure.");
        }
    }
}