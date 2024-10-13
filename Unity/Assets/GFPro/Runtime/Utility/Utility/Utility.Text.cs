using System.Text;

namespace GFPro
{
    public static partial class Utility
    {
        /// <summary>
        /// 字符相关的实用函数。
        /// </summary>
        public static partial class Text
        {
            private const int StringBuilderCapacity = 2048;

            private static readonly StringBuilder s_CachedStringBuilder = new(StringBuilderCapacity);


            /// <summary>
            /// 获取格式化字符串。
            /// </summary>
            /// <param name="format">字符串格式。</param>
            /// <param name="args">字符串参数。</param>
            /// <returns>格式化后的字符串。</returns>
            public static string Format(string format, params object[] args)
            {
                if (format == null)
                {
                    throw new GameFrameworkException("Format is invalid.");
                }


                s_CachedStringBuilder.Length = 0;
                s_CachedStringBuilder.AppendFormat(format, args);
                return s_CachedStringBuilder.ToString();
            }
        }
    }
}