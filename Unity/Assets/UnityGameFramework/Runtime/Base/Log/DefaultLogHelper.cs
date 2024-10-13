namespace GFPro
{
    /// <summary>
    /// 默认游戏框架日志辅助器。
    /// </summary>
    public class DefaultLogHelper : ILogHelper
    {
        /// <summary>
        /// 打印调试级别日志，用于记录调试类日志信息。
        /// </summary>
        /// <param name="message">日志内容。</param>
        public void Debug(string message)
        {
            UnityEngine.Debug.Log(Utility.Text.Format("<color=#888888>{0}</color>", message));
        }

        public void Info(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        public void Error(string message)
        {
            UnityEngine.Debug.LogError(message);
        }

        public void Warning(string message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        public void Fatal(string message)
        {
            throw new GameFrameworkException(message);
        }
    }
}