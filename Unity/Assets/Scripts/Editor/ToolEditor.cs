using System.Diagnostics;
using Cysharp.Threading.Tasks;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Game.Editor
{
    public static class ToolEditor
    {
        [MenuItem("Game/Tool/ExcelExporter")]
        public static void ExcelExporter()
        {
            async UniTaskVoid RunAsync()
            {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
                const string tools = "./Tool";
#else
                const string tools = ".\\Tool.exe";
#endif
                var stopwatch = Stopwatch.StartNew();
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
                await ShellTool.RunAsync($"{tools} --AppType=ExportAllExcel", "../Bin/");
#else
                await ShellTool.RunAsync($"{tools} --AppType=ExportAllExcel --Customs=GB2312", "../Bin/");
#endif
                stopwatch.Stop();
                Debug.Log($"Export cost {stopwatch.ElapsedMilliseconds} Milliseconds!");
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                var activeObject = Selection.activeObject;
                if (activeObject != null)
                {
                    Selection.activeObject = null;
                    await UniTask.DelayFrame(2);
                    Selection.activeObject = activeObject;
                }
            }

            RunAsync().Forget();
        }

        [MenuItem("Game/Tool/ExcelExporterForJson")]
        public static void ExcelExporterForJson()
        {
            async UniTaskVoid RunAsync()
            {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
                const string tools = "./Tool";
#else
                const string tools = ".\\Tool.exe";
#endif
                var stopwatch = Stopwatch.StartNew();
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
                await ShellTool.RunAsync($"{tools} --AppType=ExportAllExcel --Customs=Json", "../Bin/");
#else
                await ShellTool.RunAsync($"{tools} --AppType=ExportAllExcel --Customs=Json,GB2312", "../Bin/");
#endif
                stopwatch.Stop();
                Debug.Log($"Export cost {stopwatch.ElapsedMilliseconds} Milliseconds!");
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                var activeObject = Selection.activeObject;
                if (activeObject != null)
                {
                    Selection.activeObject = null;
                    await UniTask.DelayFrame(2);
                    Selection.activeObject = activeObject;
                }
            }

            RunAsync().Forget();
        }
    }
}