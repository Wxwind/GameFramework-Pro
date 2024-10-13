using Cysharp.Threading.Tasks;

namespace GFPro
{
    /// <summary>
    /// 数据提供者接口。
    /// </summary>
    public interface IDataProvider
    {
        /// <summary>
        /// 读取数据。
        /// </summary>
        /// <param name="dataAssetName">内容资源名称。</param>
        UniTask ReadData(string dataAssetName);

        /// <summary>
        /// 解析内容。
        /// </summary>
        /// <param name="dataString">要解析的内容字符串。</param>
        /// <returns>是否解析内容成功。</returns>
        void ParseData(string dataString);

        /// <summary>
        /// 解析内容。
        /// </summary>
        /// <param name="dataBytes">要解析的内容二进制流。</param>
        /// <returns>是否解析内容成功。</returns>
        void ParseData(byte[] dataBytes);

        /// <summary>
        /// 解析内容。
        /// </summary>
        /// <param name="dataBytes">要解析的内容二进制流。</param>
        /// <param name="startIndex">内容二进制流的起始位置。</param>
        /// <param name="length">内容二进制流的长度。</param>
        /// <returns>是否解析内容成功。</returns>
        void ParseData(byte[] dataBytes, int startIndex, int length);
    }
}