using System.Collections.Generic;
using System.IO;
using System.Xml;
using GameFramework;
using UnityEditor;
using UnityEngine;

namespace UnityGameFramework.Editor
{
    /// <summary>
    /// 构建配置相关的实用函数。
    /// </summary>
    internal static class BuildSettings
    {
        private static readonly string       s_ConfigurationPath = null;
        private static readonly List<string> s_DefaultSceneNames = new();
        private static readonly List<string> s_SearchScenePaths  = new();

        static BuildSettings()
        {
            s_ConfigurationPath = Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "AssetRes/Editor/Configs/BuildSettings.xml"));
            s_DefaultSceneNames.Clear();
            s_SearchScenePaths.Clear();

            if (!File.Exists(s_ConfigurationPath))
            {
                return;
            }

            try
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.Load(s_ConfigurationPath);
                var xmlRoot = xmlDocument.SelectSingleNode("UnityGameFramework");
                var xmlBuildSettings = xmlRoot.SelectSingleNode("BuildSettings");
                var xmlDefaultScenes = xmlBuildSettings.SelectSingleNode("DefaultScenes");
                var xmlSearchScenePaths = xmlBuildSettings.SelectSingleNode("SearchScenePaths");

                XmlNodeList xmlNodeList = null;
                XmlNode xmlNode = null;

                xmlNodeList = xmlDefaultScenes.ChildNodes;
                for (int i = 0; i < xmlNodeList.Count; i++)
                {
                    xmlNode = xmlNodeList.Item(i);
                    if (xmlNode.Name != "DefaultScene")
                    {
                        continue;
                    }

                    string defaultSceneName = xmlNode.Attributes.GetNamedItem("Name").Value;
                    s_DefaultSceneNames.Add(defaultSceneName);
                }

                xmlNodeList = xmlSearchScenePaths.ChildNodes;
                for (int i = 0; i < xmlNodeList.Count; i++)
                {
                    xmlNode = xmlNodeList.Item(i);
                    if (xmlNode.Name != "SearchScenePath")
                    {
                        continue;
                    }

                    string searchScenePath = xmlNode.Attributes.GetNamedItem("Path").Value;
                    s_SearchScenePaths.Add(searchScenePath);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// 将构建场景设置为默认。
        /// </summary>
        [MenuItem("Game Framework/Scenes in Build Settings/Default Scenes", false, 20)]
        public static void DefaultScenes()
        {
            var sceneNames = new HashSet<string>();
            foreach (string sceneName in s_DefaultSceneNames)
            {
                sceneNames.Add(sceneName);
            }

            var scenes = new List<EditorBuildSettingsScene>();
            foreach (string sceneName in sceneNames)
            {
                scenes.Add(new EditorBuildSettingsScene(sceneName, true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();

            Debug.Log("Set scenes of build settings to default scenes.");
        }

        /// <summary>
        /// 将构建场景设置为所有。
        /// </summary>
        [MenuItem("Game Framework/Scenes in Build Settings/All Scenes", false, 21)]
        public static void AllScenes()
        {
            var sceneNames = new HashSet<string>();
            foreach (string sceneName in s_DefaultSceneNames)
            {
                sceneNames.Add(sceneName);
            }

            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", s_SearchScenePaths.ToArray());
            foreach (string sceneGuid in sceneGuids)
            {
                string sceneName = AssetDatabase.GUIDToAssetPath(sceneGuid);
                sceneNames.Add(sceneName);
            }

            var scenes = new List<EditorBuildSettingsScene>();
            foreach (string sceneName in sceneNames)
            {
                scenes.Add(new EditorBuildSettingsScene(sceneName, true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();

            Debug.Log("Set scenes of build settings to all scenes.");
        }
    }
}