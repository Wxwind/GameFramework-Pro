namespace UnityGameFramework.Editor.ResourceTools
{
    /// <summary>
    /// 对 AssetBundle 应用的压缩算法类型。
    /// </summary>
    public enum AssetBundleCompressionType : byte
    {
        /// <summary>
        /// 不使用压缩算法。
        /// </summary>
        Uncompressed = 0,

        /// <summary>
        /// 使用 LZ4 压缩算法。
        /// </summary>
        LZ4,

        /// <summary>
        /// 使用 LZMA 压缩算法。
        /// </summary>
        LZMA
    }
}
