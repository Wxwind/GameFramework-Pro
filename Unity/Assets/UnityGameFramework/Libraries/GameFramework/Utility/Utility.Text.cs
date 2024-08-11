namespace GameFramework
{
    public static partial class Utility
    {
        /// <summary>
        /// 字符相关的实用函数。
        /// </summary>
        public static partial class Text
        {
            private static ITextHelper s_TextHelper;

            /// <summary>
            /// 设置字符辅助器。
            /// </summary>
            /// <param name="textHelper">要设置的字符辅助器。</param>
            public static void SetTextHelper(ITextHelper textHelper)
            {
                s_TextHelper = textHelper;
            }

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

                if (s_TextHelper == null)
                {
                    return string.Format(format, args);
                }

                return s_TextHelper.Format(format, args);
            }
        }
    }
}