using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Localization;

namespace Game
{
    public class SettingForm : UGuiForm
    {
        [SerializeField] private Toggle m_MusicMuteToggle;

        [SerializeField] private Slider m_MusicVolumeSlider;

        [SerializeField] private Toggle m_SoundMuteToggle;

        [SerializeField] private Slider m_SoundVolumeSlider;

        [SerializeField] private Toggle m_UISoundMuteToggle;

        [SerializeField] private Slider m_UISoundVolumeSlider;

        [SerializeField] private Toggle m_EnglishToggle;

        [SerializeField] private Toggle m_ChineseSimplifiedToggle;

        [SerializeField] private Toggle m_ChineseTraditionalToggle;

        [SerializeField] private Toggle m_KoreanToggle;

        private Language m_SelectedLanguage = Language.Unspecified;

        public void OnMusicMuteChanged(bool isOn)
        {
            GameEntry.Sound.Mute("Music", !isOn);
            m_MusicVolumeSlider.gameObject.SetActive(isOn);
        }

        public void OnMusicVolumeChanged(float volume)
        {
            GameEntry.Sound.SetVolume("Music", volume);
        }

        public void OnSoundMuteChanged(bool isOn)
        {
            GameEntry.Sound.Mute("Sound", !isOn);
            m_SoundVolumeSlider.gameObject.SetActive(isOn);
        }

        public void OnSoundVolumeChanged(float volume)
        {
            GameEntry.Sound.SetVolume("Sound", volume);
        }

        public void OnUISoundMuteChanged(bool isOn)
        {
            GameEntry.Sound.Mute("UISound", !isOn);
            m_UISoundVolumeSlider.gameObject.SetActive(isOn);
        }

        public void OnUISoundVolumeChanged(float volume)
        {
            GameEntry.Sound.SetVolume("UISound", volume);
        }

        public void OnEnglishSelected(bool isOn)
        {
            if (!isOn) return;

            m_SelectedLanguage = Language.English;
            OnLanguageSelect();
        }

        public void OnChineseSimplifiedSelected(bool isOn)
        {
            if (!isOn) return;

            m_SelectedLanguage = Language.ChineseSimplified;
            OnLanguageSelect();
        }

        public void OnChineseTraditionalSelected(bool isOn)
        {
            if (!isOn) return;

            m_SelectedLanguage = Language.ChineseTraditional;
            OnLanguageSelect();
        }

        public void OnKoreanSelected(bool isOn)
        {
            if (!isOn) return;

            m_SelectedLanguage = Language.Korean;
            OnLanguageSelect();
        }

        public void OnSubmitButtonClick()
        {
            var savedLanguage = Enum.Parse<Language>(GameEntry.Setting.GetString(Constant.Setting.Language));
            if (m_SelectedLanguage == savedLanguage)
            {
                Close();
                return;
            }

            GameEntry.Setting.SetString(Constant.Setting.Language, m_SelectedLanguage.ToString());
            GameEntry.Setting.Save();
            Close();
        }

        public void OnCancelButtonClick()
        {
            GameEntry.Localization.SetLanguage(Enum.Parse<Language>(GameEntry.Setting.GetString(Constant.Setting.Language))).Forget();
            Close(false);
        }

        protected override void OnOpen(object userData)
        {
            // 在GameObject SetActive前先把默认值设置好，避免触发ToggleGroup的默认选中第一个逻辑
            m_SelectedLanguage = GameEntry.Localization.Language;

            m_MusicMuteToggle.isOn = !GameEntry.Sound.IsMuted("Music");
            m_MusicVolumeSlider.value = GameEntry.Sound.GetVolume("Music");

            m_SoundMuteToggle.isOn = !GameEntry.Sound.IsMuted("Sound");
            m_SoundVolumeSlider.value = GameEntry.Sound.GetVolume("Sound");

            m_UISoundMuteToggle.isOn = !GameEntry.Sound.IsMuted("UISound");
            m_UISoundVolumeSlider.value = GameEntry.Sound.GetVolume("UISound");

            switch (m_SelectedLanguage)
            {
                case Language.English:
                    m_EnglishToggle.isOn = true;
                    break;

                case Language.ChineseSimplified:
                    m_ChineseSimplifiedToggle.isOn = true;
                    break;

                case Language.ChineseTraditional:
                    m_ChineseTraditionalToggle.isOn = true;
                    break;

                case Language.Korean:
                    m_KoreanToggle.isOn = true;
                    break;
            }

            base.OnOpen(userData);
        }


        private void OnLanguageSelect()
        {
            GameEntry.Localization.SetLanguage(m_SelectedLanguage).Forget();
        }
    }
}