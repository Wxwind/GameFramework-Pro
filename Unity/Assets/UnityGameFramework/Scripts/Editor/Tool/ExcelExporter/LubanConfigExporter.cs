using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Editor
{
    public static class LubanConfigExporter
    {
        private static readonly string ProjectRootPath = Path.Combine(Environment.CurrentDirectory, "../");

        [MenuItem("Excel/Generate DataTables")]
        private static void GenerateDataTables()
        {
            ShellExecutor.RunCmd(Path.Combine(ProjectRootPath, "Tools/Luban"), RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "genJson.bat" : "genJson.sh");
        }

        [MenuItem("Excel/Open Folder %E")]
        private static void OpenExcelFolder()
        {
            OpenFolder.Execute(Path.Combine(ProjectRootPath, "Excel"));
        }
    }
}