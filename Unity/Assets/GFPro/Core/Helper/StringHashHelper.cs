using System;

namespace GFPro
{
    public static class StringHashHelper
    {
        // bkdr hash
        public static long GetLongHashCode(this string str)
        {
            const uint seed = 1313; // 31 131 1313 13131 131313 etc..

            ulong hash = 0;
            for (var i = 0; i < str.Length; ++i)
            {
                var c = str[i];
                var high = (byte)(c >> 8);
                var low = (byte)(c & byte.MaxValue);
                hash = hash * seed + high;
                hash = hash * seed + low;
            }

            return (long)hash;
        }

        public static int Mode(this string strText, int mode)
        {
            if (mode <= 0)
            {
                throw new Exception($"string mode < 0: {strText} {mode}");
            }

            return (int)((ulong)strText.GetLongHashCode() % (uint)mode);
        }
    }
}