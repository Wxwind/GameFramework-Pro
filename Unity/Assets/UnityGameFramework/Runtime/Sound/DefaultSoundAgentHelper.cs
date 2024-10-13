using System;
using System.Collections;
using GFPro.Entity;
using GFPro.Sound;
using UnityEngine;
using UnityEngine.Audio;

namespace GFPro
{
    /// <summary>
    /// 默认声音代理辅助器。
    /// </summary>
    public class DefaultSoundAgentHelper : MonoBehaviour, ISoundAgentHelper
    {
        private Transform   m_CachedTransform;
        private AudioSource m_AudioSource;
        private EntityLogic m_BindingEntityLogic;
        private float       m_VolumeWhenPause;
        private bool        m_ApplicationPauseFlag;

        /// <summary>
        /// 获取当前是否正在播放。
        /// </summary>
        public bool IsPlaying => m_AudioSource.isPlaying;

        /// <summary>
        /// 获取声音长度。
        /// </summary>
        public float Length => m_AudioSource.clip != null ? m_AudioSource.clip.length : 0f;

        /// <summary>
        /// 获取或设置播放位置。
        /// </summary>
        public float Time
        {
            get => m_AudioSource.time;
            set => m_AudioSource.time = value;
        }

        /// <summary>
        /// 获取或设置是否静音。
        /// </summary>
        public bool Mute
        {
            get => m_AudioSource.mute;
            set => m_AudioSource.mute = value;
        }

        /// <summary>
        /// 获取或设置是否循环播放。
        /// </summary>
        public bool Loop
        {
            get => m_AudioSource.loop;
            set => m_AudioSource.loop = value;
        }

        /// <summary>
        /// 获取或设置声音优先级。
        /// </summary>
        public int Priority
        {
            get => 128 - m_AudioSource.priority;
            set => m_AudioSource.priority = 128 - value;
        }

        /// <summary>
        /// 获取或设置音量大小。
        /// </summary>
        public float Volume
        {
            get => m_AudioSource.volume;
            set => m_AudioSource.volume = value;
        }

        /// <summary>
        /// 获取或设置声音音调。
        /// </summary>
        public float Pitch
        {
            get => m_AudioSource.pitch;
            set => m_AudioSource.pitch = value;
        }

        /// <summary>
        /// 获取或设置声音立体声声相。
        /// </summary>
        public float PanStereo
        {
            get => m_AudioSource.panStereo;
            set => m_AudioSource.panStereo = value;
        }

        /// <summary>
        /// 获取或设置声音空间混合量。
        /// </summary>
        public float SpatialBlend
        {
            get => m_AudioSource.spatialBlend;
            set => m_AudioSource.spatialBlend = value;
        }

        /// <summary>
        /// 获取或设置声音最大距离。
        /// </summary>
        public float MaxDistance
        {
            get => m_AudioSource.maxDistance;

            set => m_AudioSource.maxDistance = value;
        }

        /// <summary>
        /// 获取或设置声音多普勒等级。
        /// </summary>
        public float DopplerLevel
        {
            get => m_AudioSource.dopplerLevel;
            set => m_AudioSource.dopplerLevel = value;
        }

        /// <summary>
        /// 获取或设置声音代理辅助器所在的混音组。
        /// </summary>
        public AudioMixerGroup AudioMixerGroup
        {
            get => m_AudioSource.outputAudioMixerGroup;
            set => m_AudioSource.outputAudioMixerGroup = value;
        }

        /// <summary>
        /// 重置声音代理事件。
        /// </summary>
        public event Action ResetSoundAgent;


        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
        public void Play(float fadeInSeconds)
        {
            StopAllCoroutines();

            m_AudioSource.Play();
            if (fadeInSeconds > 0f)
            {
                var volume = m_AudioSource.volume;
                m_AudioSource.volume = 0f;
                StartCoroutine(FadeToVolume(m_AudioSource, volume, fadeInSeconds));
            }
        }

        /// <summary>
        /// 停止播放声音。
        /// </summary>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        public void Stop(float fadeOutSeconds)
        {
            StopAllCoroutines();

            if (fadeOutSeconds > 0f && gameObject.activeInHierarchy)
            {
                StartCoroutine(StopCo(fadeOutSeconds));
            }
            else
            {
                m_AudioSource.Stop();
            }
        }

        /// <summary>
        /// 暂停播放声音。
        /// </summary>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        public void Pause(float fadeOutSeconds)
        {
            StopAllCoroutines();

            m_VolumeWhenPause = m_AudioSource.volume;
            if (fadeOutSeconds > 0f && gameObject.activeInHierarchy)
            {
                StartCoroutine(PauseCo(fadeOutSeconds));
            }
            else
            {
                m_AudioSource.Pause();
            }
        }

        /// <summary>
        /// 恢复播放声音。
        /// </summary>
        /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
        public void Resume(float fadeInSeconds)
        {
            StopAllCoroutines();

            m_AudioSource.UnPause();
            if (fadeInSeconds > 0f)
            {
                StartCoroutine(FadeToVolume(m_AudioSource, m_VolumeWhenPause, fadeInSeconds));
            }
            else
            {
                m_AudioSource.volume = m_VolumeWhenPause;
            }
        }

        /// <summary>
        /// 重置声音代理辅助器。
        /// </summary>
        public void Reset()
        {
            m_CachedTransform.localPosition = Vector3.zero;
            m_AudioSource.clip = null;
            m_BindingEntityLogic = null;
            m_VolumeWhenPause = 0f;
        }

        /// <summary>
        /// 设置声音资源。
        /// </summary>
        /// <param name="soundAsset">声音资源。</param>
        /// <returns>是否设置声音资源成功。</returns>
        public bool SetSoundAsset(object soundAsset)
        {
            var audioClip = soundAsset as AudioClip;
            if (audioClip == null)
            {
                return false;
            }

            m_AudioSource.clip = audioClip;
            return true;
        }

        /// <summary>
        /// 设置声音绑定的实体。
        /// </summary>
        /// <param name="bindingEntity">声音绑定的实体。</param>
        public void SetBindingEntity(IEntity bindingEntity)
        {
            m_BindingEntityLogic = (bindingEntity as Entity.Entity).Logic;
            if (m_BindingEntityLogic != null)
            {
                UpdateAgentPosition();
                return;
            }

            if (ResetSoundAgent != null)
            {
                var resetSoundAgentEventArgs = ResetSoundAgentEventArgs.Create();
                if (ResetSoundAgent != null) ResetSoundAgent();
                ReferencePool.Release(resetSoundAgentEventArgs);
            }
        }

        /// <summary>
        /// 设置声音所在的世界坐标。
        /// </summary>
        /// <param name="worldPosition">声音所在的世界坐标。</param>
        public void SetWorldPosition(Vector3 worldPosition)
        {
            m_CachedTransform.position = worldPosition;
        }

        private void Awake()
        {
            m_CachedTransform = transform;
            m_AudioSource = gameObject.GetOrAddComponent<AudioSource>();
            m_AudioSource.playOnAwake = false;
            m_AudioSource.rolloffMode = AudioRolloffMode.Custom;
        }

        private void Update()
        {
            if (!m_ApplicationPauseFlag && !IsPlaying && m_AudioSource.clip != null && ResetSoundAgent != null)
            {
                ResetSoundAgent.Invoke();
                return;
            }

            if (m_BindingEntityLogic != null)
            {
                UpdateAgentPosition();
            }
        }

        private void OnApplicationPause(bool pause)
        {
            m_ApplicationPauseFlag = pause;
        }

        private void UpdateAgentPosition()
        {
            if (m_BindingEntityLogic.Available)
            {
                m_CachedTransform.position = m_BindingEntityLogic.CachedTransform.position;
                return;
            }

            ResetSoundAgent?.Invoke();
        }

        private IEnumerator StopCo(float fadeOutSeconds)
        {
            yield return FadeToVolume(m_AudioSource, 0f, fadeOutSeconds);
            m_AudioSource.Stop();
        }

        private IEnumerator PauseCo(float fadeOutSeconds)
        {
            yield return FadeToVolume(m_AudioSource, 0f, fadeOutSeconds);
            m_AudioSource.Pause();
        }

        private IEnumerator FadeToVolume(AudioSource audioSource, float volume, float duration)
        {
            var time = 0f;
            var originalVolume = audioSource.volume;
            while (time < duration)
            {
                time += UnityEngine.Time.deltaTime;
                audioSource.volume = Mathf.Lerp(originalVolume, volume, time / duration);
                yield return new WaitForEndOfFrame();
            }

            audioSource.volume = volume;
        }
    }
}