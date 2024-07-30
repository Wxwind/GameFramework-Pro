namespace GameFramework
{
    /// <summary>
    /// 读取数据成功事件。
    /// </summary>
    public sealed class ReadDataSuccessEventArgs : GameFrameworkEventArgs
    {
        /// <summary>
        /// 初始化读取数据成功事件的新实例。
        /// </summary>
        public ReadDataSuccessEventArgs()
        {
            DataAssetName = null;
            Duration = 0f;
        }

        /// <summary>
        /// 获取内容资源名称。
        /// </summary>
        public string DataAssetName { get; private set; }

        /// <summary>
        /// 获取加载持续时间。
        /// </summary>
        public float Duration { get; private set; }


        /// <summary>
        /// 创建读取数据成功事件。
        /// </summary>
        /// <param name="dataAssetName">内容资源名称。</param>
        /// <param name="duration">加载持续时间。</param>
        /// <returns>创建的读取数据成功事件。</returns>
        public static ReadDataSuccessEventArgs Create(string dataAssetName, float duration)
        {
            ReadDataSuccessEventArgs loadDataSuccessEventArgs = ReferencePool.Acquire<ReadDataSuccessEventArgs>();
            loadDataSuccessEventArgs.DataAssetName = dataAssetName;
            loadDataSuccessEventArgs.Duration = duration;
            return loadDataSuccessEventArgs;
        }

        /// <summary>
        /// 清理读取数据成功事件。
        /// </summary>
        public override void Clear()
        {
            DataAssetName = null;
            Duration = 0f;
        }
    }
}