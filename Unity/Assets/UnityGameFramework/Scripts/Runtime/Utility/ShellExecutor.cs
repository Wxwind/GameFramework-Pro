using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Debug = UnityEngine.Debug;

namespace UnityGameFramework.Runtime
{
    public static class ShellExecutor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workingDir">运行目录</param>
        /// <param name="cmd">sh脚本</param>
        /// <param name="args">参数</param>
        /// <returns></returns>
        private static Process CreateShellExProcess(string workingDir, string cmd, string args)
        {
            var pStartInfo = new ProcessStartInfo(cmd);
            if (!string.IsNullOrEmpty(args))
            {
                pStartInfo.Arguments = args;
            }

            pStartInfo.CreateNoWindow = false;
            pStartInfo.WorkingDirectory = workingDir;


            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                pStartInfo.UseShellExecute = true;
            }
            else
            {
                pStartInfo.UseShellExecute = false;
            }

            if (pStartInfo.UseShellExecute)
            {
                pStartInfo.RedirectStandardOutput = false;
                pStartInfo.RedirectStandardError = false;
                pStartInfo.RedirectStandardInput = false;
            }
            else
            {
                pStartInfo.RedirectStandardOutput = true;
                pStartInfo.RedirectStandardError = true;
                pStartInfo.RedirectStandardInput = true;
                pStartInfo.StandardOutputEncoding = Encoding.UTF8;
                pStartInfo.StandardErrorEncoding = Encoding.UTF8;
            }


            var process = Process.Start(pStartInfo);
            if (pStartInfo.UseShellExecute == false)
            {
                if (process != null)
                {
                    string output = "输出信息:" + process.StandardOutput.ReadToEnd();
                    Debug.Log(output);
                    string outError = process.StandardError.ReadToEnd();
                    if (outError == "")
                    {
                        Debug.Log("执行成功");
                    }
                    else Debug.LogError(outError);
                }
                else
                {
                    Debug.Log("process is null");
                }
            }

            return process;
        }

        public static void RunCmd(string path, string cmdFileName, string args = "")
        {
            string batFileFullPath = Path.Combine(path, cmdFileName);
            if (!File.Exists(batFileFullPath))
            {
                Debug.LogError($"{cmdFileName}文件不存在此目录中:{path}");
            }
            else
            {
                try
                {
                    var p = CreateShellExProcess(path, batFileFullPath, args);
                    p.WaitForExit();
                    p.Close();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
    }
}