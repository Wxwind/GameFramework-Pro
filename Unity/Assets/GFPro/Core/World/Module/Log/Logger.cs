using System;
using System.Diagnostics;

namespace GFPro
{
    public class Logger : Singleton<Logger>, ISingletonAwake
    {
        private ILog iLog;

        public ILog ILog
        {
            set => iLog = value;
        }

        private const int TraceLevel   = 1;
        private const int DebugLevel   = 2;
        private const int InfoLevel    = 3;
        private const int WarningLevel = 4;

        private bool CheckLogLevel(int level)
        {
            if (Options.Instance == null)
            {
                return true;
            }

            return Options.Instance.LogLevel <= level;
        }

        public void Trace(string msg)
        {
            if (!CheckLogLevel(DebugLevel))
            {
                return;
            }

            var st = new StackTrace(2, true);
            iLog.Trace($"{msg}\n{st}");
        }

        public void Debug(string msg)
        {
            if (!CheckLogLevel(DebugLevel))
            {
                return;
            }

            iLog.Debug(msg);
        }

        public void Info(string msg)
        {
            if (!CheckLogLevel(InfoLevel))
            {
                return;
            }

            iLog.Info(msg);
        }

        public void TraceInfo(string msg)
        {
            if (!CheckLogLevel(InfoLevel))
            {
                return;
            }

            var st = new StackTrace(2, true);
            iLog.Trace($"{msg}\n{st}");
        }

        public void Warning(string msg)
        {
            if (!CheckLogLevel(WarningLevel))
            {
                return;
            }

            iLog.Warning(msg);
        }

        public void Error(string msg)
        {
            var st = new StackTrace(2, true);
            iLog.Error($"{msg}\n{st}");
        }

        public void Error(Exception e)
        {
            if (e.Data.Contains("StackTrace"))
            {
                iLog.Error($"{e.Data["StackTrace"]}\n{e}");
                return;
            }

            var str = e.ToString();
            iLog.Error(str);
        }

        public void Trace(string message, params object[] args)
        {
            if (!CheckLogLevel(TraceLevel))
            {
                return;
            }

            var st = new StackTrace(2, true);
            iLog.Trace($"{string.Format(message, args)}\n{st}");
        }

        public void Warning(string message, params object[] args)
        {
            if (!CheckLogLevel(WarningLevel))
            {
                return;
            }

            iLog.Warning(string.Format(message, args));
        }

        public void Info(string message, params object[] args)
        {
            if (!CheckLogLevel(InfoLevel))
            {
                return;
            }

            iLog.Info(string.Format(message, args));
        }

        public void Debug(string message, params object[] args)
        {
            if (!CheckLogLevel(DebugLevel))
            {
                return;
            }

            iLog.Debug(string.Format(message, args));
        }

        public void Error(string message, params object[] args)
        {
            var st = new StackTrace(2, true);
            var s = string.Format(message, args) + '\n' + st;
            iLog.Error(s);
        }

        public void Console(string message)
        {
            if (Options.Instance.Console == 1)
            {
                System.Console.WriteLine(message);
            }

            iLog.Debug(message);
        }

        public void Console(string message, params object[] args)
        {
            var s = string.Format(message, args);
            if (Options.Instance.Console == 1)
            {
                System.Console.WriteLine(s);
            }

            iLog.Debug(s);
        }

        public void Awake()
        {
        }
    }
}