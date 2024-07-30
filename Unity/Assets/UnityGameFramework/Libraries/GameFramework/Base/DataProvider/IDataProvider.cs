using System;
using Cysharp.Threading.Tasks;

namespace GameFramework
{
    /// <summary>
    /// 数据提供者接口。
    /// </summary>
    /// <typeparam name="T">数据提供者的持有者的类型。</typeparam>
    public interface IDataProvider<T>
    {
        /// <summary>
        /// 读取数据成功事件。
        /// </summary>
        event EventHandler<ReadDataSuccessEventArgs> ReadDataSuccess;

        /// <summary>
        /// 读取数据失败事件。
        /// </summary>
        event EventHandler<ReadDataFailureEventArgs> ReadDataFailure;


        /// <summary>
        /// 读取数据。
        /// </summary>
        /// <param name="dataAssetName">内容资源名称。</param>
        UniTask ReadData(string dataAssetName);

        /// <summary>
        /// 读取数据。
        /// </summary>
        /// <param name="dataAssetName">内容资源名称。</param>
        /// <param name="priority">加载数据资源的优先级。</param>
        UniTask ReadData(string dataAssetName, int priority);


        /// <summary>
        /// 解析内容。
        /// </summary>
        /// <param name="dataString">要解析的内容字符串。</param>
        /// <returns>是否解析内容成功。</returns>
        bool ParseData(string dataString);

        /// <summary>
        /// 解析内容。
        /// </summary>
        /// <param name="dataBytes">要解析的内容二进制流。</param>
        /// <returns>是否解析内容成功。</returns>
        bool ParseData(byte[] dataBytes);

        /// <summary>
        /// 解析内容。
        /// </summary>
        /// <param name="dataBytes">要解析的内容二进制流。</param>
        /// <param name="startIndex">内容二进制流的起始位置。</param>
        /// <param name="length">内容二进制流的长度。</param>
        /// <returns>是否解析内容成功。</returns>
        bool ParseData(byte[] dataBytes, int startIndex, int length);
    }
}