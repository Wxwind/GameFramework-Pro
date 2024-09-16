namespace UnityGameFramework
{
    /// <summary>
    /// 游戏框架日志辅助器接口。
    /// </summary>
    public interface ILogHelper
    {
        void Debug(string message);

        void Info(string message);

        void Error(string message);

        void Warning(string message);

        void Fatal(string message);
    }
}