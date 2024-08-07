using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Tool
{
    public static partial class ExcelExporter
    {
        public static class ExcelExporter_Luban
        {
            private const string GEN_CLIENT          = "../Tools/Luban/Source/Luban.dll";
            private const string CUSTOM_TEMPLATE_DIR = "../Tools/Luban/CustomTemplates";
            private const string EXCEL_DIR           = "../Config/Excel";
            private const string GEN_SHELL_TEMPLATE  = "shell-template.json";

            /// <summary>
            ///     luban命令模板，不能带换行符
            /// </summary>
            private static readonly string s_LubanCommandHeaderTemplate =
                "dotnet %GEN_CLIENT% --customTemplateDir %CUSTOM_TEMPLATE_DIR% --conf %CONF_ROOT%/luban.conf ";

            private static Encoding s_Encoding;


            public static void DoExport()
            {
                var isGB2312 = Options.Instance.Customs.Contains("GB2312", StringComparison.OrdinalIgnoreCase);
                var useJson = Options.Instance.Customs.Contains("Json", StringComparison.OrdinalIgnoreCase);
                var isCheck = Options.Instance.Customs.Contains("Check", StringComparison.OrdinalIgnoreCase);
                var showCmd = Options.Instance.Customs.Contains("ShowCmd", StringComparison.OrdinalIgnoreCase);
                var showInfo = Options.Instance.Customs.Contains("ShowInfo", StringComparison.OrdinalIgnoreCase);

                var actionStr = isCheck ? "check" : "export";
                Log.Info($"Start {actionStr} Luban excel ...");
                if (isGB2312)
                {
                    //luban在windows上编码为GB2312
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    s_Encoding = Encoding.GetEncoding("GB2312");
                }
                else
                {
                    s_Encoding = Encoding.UTF8;
                }

                var excelDir = Path.GetFullPath(Path.Combine(Define.WorkDir, EXCEL_DIR));
                var dirs = Directory.GetDirectories(excelDir);
                if (dirs.Length < 1) throw new Exception($"Directory {excelDir} is empty");

                var dirList = dirs.ToList();
                dirList.Sort();
                dirs = dirList.ToArray();

                var cmdInfos = new List<CmdInfo>();
                for (var i = 0; i < dirs.Length; i++)
                {
                    var dir = Path.GetFullPath(dirs[i]);
                    var genConfigFile = Path.Combine(dir, GEN_SHELL_TEMPLATE);
                    if (!File.Exists(genConfigFile)) continue;

                    var genConfig = JsonSerializer.Deserialize<GenConfig>(
                        File.ReadAllText(genConfigFile, Encoding.UTF8).Replace("\r\n", " ").Replace("\n", " ").Replace("\u0009", " "),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    var lastIndex = dir.LastIndexOf(Path.DirectorySeparatorChar) + 1;
                    var dirName = dir.Substring(lastIndex, dir.Length - lastIndex);
                    var customTemplate = string.IsNullOrEmpty(genConfig.customTemplate) ? "Default" : genConfig.customTemplate;
                    for (var j = 0; j < genConfig.cmds.Count; j++)
                    {
                        var cmdInfo = new CmdInfo();
                        var cmd = s_LubanCommandHeaderTemplate + genConfig.cmds[j];
                        cmd = cmd
                            .Replace("%GEN_CLIENT%", Path.GetFullPath(Path.Combine(Define.WorkDir, GEN_CLIENT)))
                            .Replace("%CUSTOM_TEMPLATE_DIR%", Path.GetFullPath(Path.Combine(Define.WorkDir, $"{CUSTOM_TEMPLATE_DIR}/{customTemplate}")))
                            .Replace("%CONF_ROOT%", dir)
                            .Replace("%UNITY_ASSETS%", Path.GetFullPath(Path.Combine(Define.WorkDir, Define.UNITY_ASSETS_PATH)))
                            .Replace("%ROOT%", Path.GetFullPath(Path.Combine(Define.WorkDir, Define.ROOT_PATH)))
                            .Replace('\\', '/');

                        //去掉连续多个空格
                        var match = Regex.Match(cmd, "-(.*)");
                        if (match.Success)
                        {
                            var hyphenAndAfter = match.Value;
                            var replaced = Regex.Replace(hyphenAndAfter, @"(\s+)", " ");
                            cmd = cmd.Replace(hyphenAndAfter, replaced);
                        }

                        if (useJson)
                            cmd = cmd
                                .Replace("-c cs-bin", "-c cs-simple-json")
                                .Replace("-d bin", "-d json");

                        if (isCheck)
                        {
                            // cmd = Regex.Replace(cmd, @"-x\s.*outputCodeDir\s*=\S*\s", "");
                            // cmd = Regex.Replace(cmd, @"-x\s.*outputDataDir\s*=\S*\s", "");
                            // cmd = Regex.Replace(cmd, @"-c\s\S+\s", "");
                            // cmd = Regex.Replace(cmd, @"-d\s\S+\s", "");
                            // cmd += " -f";
                            cmd += " -x outputSaver=null";
                        }

                        if (!cmd.Contains("l10n.provider")) cmd += " -x l10n.provider=default";
                        if (!cmd.Contains("l10n.textFile.path")) cmd += $" -x l10n.textFile.path={s_LocalizationExcelFile}";
                        if (!cmd.Contains("l10n.textFile.keyFieldName")) cmd += " -x l10n.textFile.keyFieldName=key";

                        cmd = Regex.Replace(cmd, @"\s+(?=-)", " ");

                        match = Regex.Match(cmd, @"-x\s.*outputCodeDir\s*=([^\s]*)");
                        if (match.Success)
                        {
                            var pathStr = match.Groups[1].Value.Trim();
                            var paths = pathStr.Split(',');
                            if (paths.Length > 1)
                            {
                                cmdInfo.sourceCodePath = paths[0];
                                cmd = cmd.Replace(pathStr, cmdInfo.sourceCodePath);
                                cmdInfo.copyCodePath = new List<string>();
                                for (var k = 1; k < paths.Length; k++) cmdInfo.copyCodePath.Add(paths[k]);
                            }
                        }

                        match = Regex.Match(cmd, @"-x\s.*outputDataDir\s*=([^\s]*)");
                        if (match.Success)
                        {
                            var pathStr = match.Groups[1].Value.Trim();
                            var paths = pathStr.Split(',');
                            if (paths.Length > 1)
                            {
                                cmdInfo.sourceDataPath = paths[0];
                                cmd = cmd.Replace(pathStr, cmdInfo.sourceDataPath);
                                cmdInfo.copyDataPath = new List<string>();
                                for (var k = 1; k < paths.Length; k++) cmdInfo.copyDataPath.Add(paths[k]);
                            }
                        }

                        cmdInfo.cmd = cmd;
                        cmdInfo.dirName = dirName;
                        cmdInfos.Add(cmdInfo);
                    }
                }

                var isSuccess = true;
                var processCount = 0;
                Parallel.ForEachAsync(cmdInfos,
                    async (cmdInfo, _) =>
                    {
                        if (showCmd) Log.Info($"{cmdInfo.dirName}: {cmdInfo.cmd}");

                        if (await RunCommand(cmdInfo.cmd, Define.WorkDir, cmdInfo.dirName, showInfo))
                        {
                            Log.Info($"{cmdInfo.dirName}: Luban {actionStr} process {Interlocked.Add(ref processCount, 1)}/{cmdInfos.Count}");
                        }
                        else
                        {
                            isSuccess = false;
                            Log.Warning($"{cmdInfo.dirName}: Luban {actionStr} process {Interlocked.Add(ref processCount, 1)}/{cmdInfos.Count}");
                        }
                    }).Wait();

                if (!isCheck)
                    foreach (var cmdInfo in cmdInfos)
                    {
                        LubanFileHelper.ClearSubEmptyDirectory(cmdInfo.sourceCodePath);
                        LubanFileHelper.ClearSubEmptyDirectory(cmdInfo.sourceDataPath);
                        if (cmdInfo.copyCodePath != null)
                            foreach (var copyPath in cmdInfo.copyCodePath)
                            {
                                LubanFileHelper.CopyDirectory(cmdInfo.sourceCodePath, copyPath);
                                LubanFileHelper.ClearSubEmptyDirectory(copyPath);
                            }

                        if (cmdInfo.copyDataPath != null)
                            foreach (var copyPath in cmdInfo.copyDataPath)
                            {
                                LubanFileHelper.CopyDirectory(cmdInfo.sourceDataPath, copyPath);
                                LubanFileHelper.ClearSubEmptyDirectory(copyPath);
                            }
                    }

                if (isSuccess)
                    Log.Info($"Luban excel {actionStr} success!");
                else
                    Log.Warning($"Luban excel {actionStr} fail!");
            }

            private static async Task<bool> RunCommand(string cmd, string workDir, string logHeader, bool showInfo)
            {
                var isSuccess = true;
                Process process = new();
                try
                {
                    var app = "bash";
                    var arguments = "-c";
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        app = "cmd.exe";
                        arguments = "/c";
                    }

                    var start = new ProcessStartInfo(app);

                    process.StartInfo = start;
                    start.Arguments = arguments + " \"" + cmd + "\"";
                    start.CreateNoWindow = true;
                    start.ErrorDialog = true;
                    start.UseShellExecute = false;
                    start.WorkingDirectory = workDir;

                    start.RedirectStandardOutput = true;
                    start.RedirectStandardError = true;
                    start.RedirectStandardInput = true;
                    start.StandardOutputEncoding = s_Encoding;
                    start.StandardErrorEncoding = s_Encoding;

                    process.OutputDataReceived += (sender, args) =>
                    {
                        if (showInfo && !string.IsNullOrEmpty(args.Data)) Log.Info(args.Data);
                    };
                    process.ErrorDataReceived += (sender, args) =>
                    {
                        if (!string.IsNullOrEmpty(args.Data))
                        {
                            isSuccess = false;
                            Log.Warning($"{logHeader} : {args.Data}");
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    await process.WaitForExitAsync();
                }
                catch (Exception e)
                {
                    isSuccess = false;
                    Log.Error(e);
                }
                finally
                {
                    process.Close();
                }

                return isSuccess;
            }

            private class CmdInfo
            {
                public string       cmd;
                public List<string> copyCodePath;
                public List<string> copyDataPath;
                public string       dirName;
                public string       sourceCodePath;
                public string       sourceDataPath;
            }

            public class GenConfig
            {
                public string customTemplate { get; set; }
                public List<string> cmds { get; set; }
            }
        }
    }
}