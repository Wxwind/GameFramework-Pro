using GameFramework.Sound;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 默认声音辅助器。
    /// </summary>
    public class DefaultSoundHelper : ISoundHelper
    {
        private ResourceComponent m_ResourceComponent = null;

        public DefaultSoundHelper()
        {
            m_ResourceComponent = GameEntry.GetComponent<ResourceComponent>();
            if (m_ResourceComponent == null)
            {
                Log.Fatal("Resource component is invalid.");
                return;
            }
        }

        /// <summary>
        /// 释放声音资源。
        /// </summary>
        /// <param name="soundAsset">要释放的声音资源。</param>
        public void ReleaseSoundAsset(Object soundAsset)
        {
            m_ResourceComponent.UnloadAsset(soundAsset);
        }
    }
}