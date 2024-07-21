using System;
using System.IO;
using UnityEditor;
using UnityGameFramework.Editor;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Scripts.Editor.LubanConfig
{
    public class LubanConfigGeneratorMenu
    {
        private static readonly string ProjectRootPath = Path.Combine(Environment.CurrentDirectory, "../");

        [MenuItem("Excel/Generate DataTables")]
        private static void GenerateDataTables()
        {
            ShellExecutor.RunCmd(ProjectRootPath + "/Tools/Luban", "genJson.bat");
        }

        [MenuItem("Excel/Open Folder %E")]
        private static void OpenExcelFolder()
        {
            OpenFolder.Execute(Path.Combine(ProjectRootPath, "Excel"));
        }
    }
}