﻿namespace GFPro.Sound
{
    /// <summary>
    /// 重置声音代理事件。
    /// </summary>
    public sealed class ResetSoundAgentEventArgs : GameFrameworkEventArgs
    {
        /// <summary>
        /// 初始化重置声音代理事件的新实例。
        /// </summary>
        public ResetSoundAgentEventArgs()
        {
        }

        /// <summary>
        /// 创建重置声音代理事件。
        /// </summary>
        /// <returns>创建的重置声音代理事件。</returns>
        public static ResetSoundAgentEventArgs Create()
        {
            var resetSoundAgentEventArgs = ReferencePool.Acquire<ResetSoundAgentEventArgs>();
            return resetSoundAgentEventArgs;
        }

        /// <summary>
        /// 清理重置声音代理事件。
        /// </summary>
        public override void Clear()
        {
        }
    }
}