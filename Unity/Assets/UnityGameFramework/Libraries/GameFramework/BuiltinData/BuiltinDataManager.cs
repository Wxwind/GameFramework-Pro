namespace GameFramework.BuiltinData
{
    internal class BuiltinDataManager : GameFrameworkModule, IBuiltinDataManager
    {
        public BuildInfo BuildInfo { get; set; }


        /// <summary>
        /// 全局配置管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        /// <summary>
        /// 关闭并清理全局配置管理器。
        /// </summary>
        internal override void Shutdown()
        {
        }
    }
}