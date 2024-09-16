using UnityGameFramework;
using UnityGameFramework.Runtime;
using UnityGameFramework.Sound;

namespace Game
{
    public static class SoundExtension
    {
        private const  float FadeVolumeDuration = 1f;
        private static int?  s_MusicSerialId;

        public static int? PlayMusic(this SoundComponent soundComponent, int musicId)
        {
            soundComponent.StopMusic();
            var tbMusic = GameEntry.LubanDataTable.Tables.TbMusic;
            var drMusic = tbMusic.GetOrDefault(musicId);
            if (drMusic == null)
            {
                Log.Warning("Can not load music '{0}' from data table.", musicId.ToString());
                return null;
            }

            var playSoundParams = PlaySoundParams.Create();
            playSoundParams.Priority = 64;
            playSoundParams.Loop = true;
            playSoundParams.VolumeInSoundGroup = 1f;
            playSoundParams.FadeInSeconds = FadeVolumeDuration;
            playSoundParams.SpatialBlend = 0f;
            s_MusicSerialId = soundComponent.PlaySound(drMusic.AssetName, "Music", playSoundParams, null);
            return s_MusicSerialId;
        }

        public static void StopMusic(this SoundComponent soundComponent)
        {
            if (!s_MusicSerialId.HasValue) return;

            soundComponent.StopSound(s_MusicSerialId.Value, FadeVolumeDuration);
            s_MusicSerialId = null;
        }

        public static int? PlaySound(this SoundComponent soundComponent, int soundId, Entity bindingEntity = null)
        {
            var tbSound = GameEntry.LubanDataTable.Tables.TbSound;
            var drSound = tbSound.GetOrDefault(soundId);
            if (drSound == null)
            {
                Log.Warning("Can not load sound '{0}' from data table.", soundId.ToString());
                return null;
            }

            var playSoundParams = PlaySoundParams.Create();
            playSoundParams.Priority = drSound.Priority;
            playSoundParams.Loop = drSound.Loop;
            playSoundParams.VolumeInSoundGroup = drSound.Volume;
            playSoundParams.SpatialBlend = drSound.SpatialBlend;
            return soundComponent.PlaySound(drSound.AssetName, "Sound",
                playSoundParams,
                bindingEntity != null ? bindingEntity.Entity : null);
        }

        public static int? PlayUISound(this SoundComponent soundComponent, int uiSoundId)
        {
            var tbUISound = GameEntry.LubanDataTable.Tables.TbUISound;
            var drUISound = tbUISound.GetOrDefault(uiSoundId);
            if (drUISound == null)
            {
                Log.Warning("Can not load UI sound '{0}' from data table.", uiSoundId.ToString());
                return null;
            }

            var playSoundParams = PlaySoundParams.Create();
            playSoundParams.Priority = drUISound.Priority;
            playSoundParams.Loop = false;
            playSoundParams.VolumeInSoundGroup = drUISound.Volume;
            playSoundParams.SpatialBlend = 0f;
            return soundComponent.PlaySound(drUISound.AssetName, "UISound",
                playSoundParams);
        }

        public static bool IsMuted(this SoundComponent soundComponent, string soundGroupName)
        {
            if (string.IsNullOrEmpty(soundGroupName))
            {
                Log.Warning("Sound group is invalid.");
                return true;
            }

            var soundGroup = soundComponent.GetSoundGroup(soundGroupName);
            if (soundGroup == null)
            {
                Log.Warning("Sound group '{0}' is invalid.", soundGroupName);
                return true;
            }

            return soundGroup.Mute;
        }

        public static void Mute(this SoundComponent soundComponent, string soundGroupName, bool mute)
        {
            if (string.IsNullOrEmpty(soundGroupName))
            {
                Log.Warning("Sound group is invalid.");
                return;
            }

            var soundGroup = soundComponent.GetSoundGroup(soundGroupName);
            if (soundGroup == null)
            {
                Log.Warning("Sound group '{0}' is invalid.", soundGroupName);
                return;
            }

            soundGroup.Mute = mute;

            GameEntry.Setting.SetBool(Utility.Text.Format(Constant.Setting.SoundGroupMuted, soundGroupName), mute);
            GameEntry.Setting.Save();
        }

        public static float GetVolume(this SoundComponent soundComponent, string soundGroupName)
        {
            if (string.IsNullOrEmpty(soundGroupName))
            {
                Log.Warning("Sound group is invalid.");
                return 0f;
            }

            var soundGroup = soundComponent.GetSoundGroup(soundGroupName);
            if (soundGroup == null)
            {
                Log.Warning("Sound group '{0}' is invalid.", soundGroupName);
                return 0f;
            }

            return soundGroup.Volume;
        }

        public static void SetVolume(this SoundComponent soundComponent, string soundGroupName, float volume)
        {
            if (string.IsNullOrEmpty(soundGroupName))
            {
                Log.Warning("Sound group is invalid.");
                return;
            }

            var soundGroup = soundComponent.GetSoundGroup(soundGroupName);
            if (soundGroup == null)
            {
                Log.Warning("Sound group '{0}' is invalid.", soundGroupName);
                return;
            }

            soundGroup.Volume = volume;

            GameEntry.Setting.SetFloat(Utility.Text.Format(Constant.Setting.SoundGroupVolume, soundGroupName), volume);
            GameEntry.Setting.Save();
        }
    }
}