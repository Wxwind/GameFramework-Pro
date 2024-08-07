using System;
using GameFramework;
using GameFramework.Localization;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace GameMain
{
    public class BuiltinDataComponent : GameFrameworkComponent
    {
        [SerializeField] private TextAsset m_BuildInfoTextAsset;

        [SerializeField] private TextAsset m_DefaultDictionaryTextAsset;

        [SerializeField] private UpdateResourceForm m_UpdateResourceFormTemplate;

        public BuildInfo BuildInfo { get; private set; }

        public UpdateResourceForm UpdateResourceFormTemplate => m_UpdateResourceFormTemplate;

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

        public void InitDefaultDictionary(Language language)
        {
            try
            {
                if (m_DefaultDictionaryTextAsset == null || string.IsNullOrEmpty(m_DefaultDictionaryTextAsset.text))
                {
                    Log.Info("Default dictionary can not be found or empty.");
                    return;
                }

                GameEntry.Localization.ParseData(m_DefaultDictionaryTextAsset.text);
            }
            catch (Exception e)
            {
                throw new GameFrameworkException("Parse default dictionary failure.", e);
            }
        }
    }
}