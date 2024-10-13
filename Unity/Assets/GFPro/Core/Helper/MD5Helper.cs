using System.IO;
using System.Security.Cryptography;

namespace GFPro
{
    public static class MD5Helper
    {
        public static string FileMD5(string filePath)
        {
            byte[] retVal;
            using (var file = new FileStream(filePath, FileMode.Open))
            {
                var md5 = MD5.Create();
                retVal = md5.ComputeHash(file);
            }

            return retVal.ToHex("x2");
        }
    }
}