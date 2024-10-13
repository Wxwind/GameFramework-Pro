using UnityEngine.Audio;

namespace GFPro.Sound
{
    /// <summary>
    /// 声音组辅助器接口。
    /// </summary>
    public interface ISoundGroupHelper
    {
        public AudioMixerGroup AudioMixerGroup { get; set; }
    }
}