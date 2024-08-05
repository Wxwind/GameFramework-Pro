using System.IO;
using GameFramework;
using UnityEngine;
using UnityGameFramework.Editor;

namespace Game.Editor
{
    public static class GameFrameworkConfigs
    {
        [BuildSettingsConfigPath] public static string BuildSettingsConfig =
            Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "AssetRes/Configs/BuildSettings.xml"));
    }
}