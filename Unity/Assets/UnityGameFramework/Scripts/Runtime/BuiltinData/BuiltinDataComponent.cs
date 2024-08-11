using GameFramework;
using GameFramework.BuiltinData;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public class BuiltinDataComponent : GameFrameworkComponent
    {
        [SerializeField] private TextAsset           m_BuildInfoTextAsset;
        private                  IBuiltinDataManager m_BuiltinDataManager;

        public BuildInfo BuildInfo => m_BuiltinDataManager.BuildInfo;


        protected override void Awake()
        {
            base.Awake();

            m_BuiltinDataManager = GameFrameworkEntry.GetModule<IBuiltinDataManager>();
            if (m_BuiltinDataManager == null)
            {
                Log.Fatal("BuiltinData manager is invalid.");
            }
        }

        public void InitBuildInfo()
        {
            if (m_BuildInfoTextAsset == null || string.IsNullOrEmpty(m_BuildInfoTextAsset.text))
            {
                Log.Info("Build info can not be found or empty.");
                return;
            }

            m_BuiltinDataManager.BuildInfo = Utility.Json.ToObject<BuildInfo>(m_BuildInfoTextAsset.text);
            if (m_BuiltinDataManager.BuildInfo == null) Log.Warning("Parse build info failure.");
        }
    }
}