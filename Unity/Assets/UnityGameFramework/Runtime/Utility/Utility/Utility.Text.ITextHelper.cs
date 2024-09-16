namespace UnityGameFramework
{
    public static partial class Utility
    {
        public static partial class Text
        {
            /// <summary>
            /// 字符辅助器接口。
            /// </summary>
            public interface ITextHelper
            {
                /// <summary>
                /// 获取格式化字符串。
                /// </summary>
                /// <param name="format">字符串格式。</param>
                /// <param name="args">字符串参数。</param>
                /// <returns>格式化后的字符串。</returns>
                string Format(string format, params object[] args);
            }
        }
    }
}