using System;
using GameFramework;
using GameFramework.Localization;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Game
{
    public class BuiltinDataComponent : GameFrameworkComponent
    {
        [SerializeField] private TextAsset m_BuildInfoTextAsset;

        public BuildInfo BuildInfo { get; private set; }

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

        public void InitBuiltinLocalization(Language language)
        {
            try
            {
                string path = $"Localization/{language}";
                var asset = Resources.Load<TextAsset>(path);
                if (asset == null)
                {
                    throw new GameFrameworkException($"{path} is invalid");
                }

                GameEntry.Localization.ParseData(asset.text);
            }
            catch (Exception e)
            {
                throw new GameFrameworkException("Parse default dictionary failure.", e);
            }
        }
    }
}