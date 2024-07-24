using System.IO;
using GameFramework;
using UnityEngine;
using UnityGameFramework.Editor;

namespace GameMain.Editor
{
    public static class GameFrameworkConfigs
    {
        [BuildSettingsConfigPath] public static string BuildSettingsConfig =
            Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "GameMain/Configs/BuildSettings.xml"));
    }
}