namespace UnityGameFramework.Sound
{
    internal sealed partial class SoundManager
    {
        private sealed class PlaySoundInfo : IReference
        {
            private int             m_SerialId;
            private SoundGroup      m_SoundGroup;
            private PlaySoundParams m_PlaySoundParams;


            public int SerialId => m_SerialId;

            public SoundGroup SoundGroup => m_SoundGroup;

            public PlaySoundParams PlaySoundParams => m_PlaySoundParams;


            public static PlaySoundInfo Create(int serialId, SoundGroup soundGroup, PlaySoundParams playSoundParams)
            {
                var playSoundInfo = ReferencePool.Acquire<PlaySoundInfo>();
                playSoundInfo.m_SerialId = serialId;
                playSoundInfo.m_SoundGroup = soundGroup;
                playSoundInfo.m_PlaySoundParams = playSoundParams;
                return playSoundInfo;
            }


            public void Clear()
            {
                m_SerialId = 0;
                m_SoundGroup = null;
                m_PlaySoundParams = null;
            }
        }
    }
}