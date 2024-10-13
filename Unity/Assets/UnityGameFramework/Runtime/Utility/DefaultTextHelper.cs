using System;
using System.Text;

namespace GFPro
{
    /// <summary>
    /// 默认字符辅助器。
    /// </summary>
    public class DefaultTextHelper : Utility.Text.ITextHelper
    {
        private const int StringBuilderCapacity = 2048;

        [ThreadStatic] private static StringBuilder s_CachedStringBuilder;

        /// <summary>
        /// 获取格式化字符串。
        /// </summary>
        /// <param name="format">字符串格式。</param>
        /// <param name="args">字符串参数。</param>
        /// <returns>格式化后的字符串。</returns>
        public string Format(string format, params object[] args)
        {
            if (format == null)
            {
                throw new GameFrameworkException("Format is invalid.");
            }

            CheckCachedStringBuilder();
            s_CachedStringBuilder.Length = 0;
            s_CachedStringBuilder.AppendFormat(format, args);
            return s_CachedStringBuilder.ToString();
        }


        private static void CheckCachedStringBuilder()
        {
            if (s_CachedStringBuilder == null)
            {
                s_CachedStringBuilder = new StringBuilder(StringBuilderCapacity);
            }
        }
    }
}