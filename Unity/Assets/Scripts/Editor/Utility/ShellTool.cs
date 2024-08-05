using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Debug = UnityEngine.Debug;

namespace Game.Editor
{
    public static class ShellTool
    {
        public static void Run(string cmd, string workDirectory, string encodingName = "UTF-8", List<string> environmentVars = null)
        {
            Process process = new();
            try
            {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
                string app = "bash";
                string splitChar = ":";
                string arguments = "-c";
#elif UNITY_EDITOR_WIN
                var app = "cmd.exe";
                var splitChar = ";";
                var arguments = "/c";
#endif
                var start = new ProcessStartInfo(app);

                if (environmentVars != null)
                {
                    foreach (var var in environmentVars)
                    {
                        start.EnvironmentVariables["PATH"] += splitChar + var;
                    }
                }

                process.StartInfo = start;
                start.Arguments = arguments + " \"" + cmd + "\"";
                start.CreateNoWindow = true;
                start.ErrorDialog = true;
                start.UseShellExecute = false;
                start.WorkingDirectory = workDirectory;

                start.RedirectStandardOutput = true;
                start.RedirectStandardError = true;
                start.RedirectStandardInput = false;
                var encoding = Encoding.GetEncoding(encodingName);
                start.StandardOutputEncoding = encoding;
                start.StandardErrorEncoding = encoding;

                process.OutputDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        Debug.Log(args.Data);
                    }
                };

                process.ErrorDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        Debug.LogError(args.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
            finally
            {
                process.Close();
            }
        }

        public static async UniTask RunAsync(string cmd, string workDirectory, string encodingName = "UTF-8", List<string> environmentVars = null)
        {
            Process process = new();
            try
            {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
                string app = "bash";
                string splitChar = ":";
                string arguments = "-c";
#elif UNITY_EDITOR_WIN
                var app = "cmd.exe";
                var splitChar = ";";
                var arguments = "/c";
#endif
                var start = new ProcessStartInfo(app);

                if (environmentVars != null)
                {
                    foreach (var var in environmentVars)
                    {
                        start.EnvironmentVariables["PATH"] += splitChar + var;
                    }
                }

                process.StartInfo = start;
                start.Arguments = arguments + " \"" + cmd + "\"";
                start.CreateNoWindow = true;
                start.ErrorDialog = true;
                start.UseShellExecute = false;
                start.WorkingDirectory = workDirectory;

                start.RedirectStandardOutput = true;
                start.RedirectStandardError = true;
                start.RedirectStandardInput = true;
                var encoding = Encoding.GetEncoding(encodingName);
                start.StandardOutputEncoding = encoding;
                start.StandardErrorEncoding = encoding;

                process.OutputDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        Debug.Log(args.Data);
                    }
                };

                process.ErrorDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        Debug.LogError(args.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await Task.Factory.StartNew(() => { process.WaitForExit(); });
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
            finally
            {
                process.Close();
            }
        }
    }
}