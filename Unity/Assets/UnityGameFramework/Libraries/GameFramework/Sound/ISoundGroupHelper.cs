using UnityEngine.Audio;

namespace GameFramework.Sound
{
    /// <summary>
    /// 声音组辅助器接口。
    /// </summary>
    public interface ISoundGroupHelper
    {
        public AudioMixerGroup AudioMixerGroup { get; set; }
    }
}