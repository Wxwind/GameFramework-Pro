using System;
using System.Linq;
using Game.Editor;
using GFPro;
using GFPro.Localization;
using GFPro.Resource;
using ProcedureOwner = GFPro.Fsm.IFsm<GFPro.Procedure.ProcedureComponent>;

namespace Game
{
    public class ProcedureLaunch : ProcedureBase
    {
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            // 构建信息：发布版本时，把一些数据以 Json 的格式写入 Assets/AssetRes/Configs/BuildInfo.json，供游戏逻辑读取
            GameEntry.BuiltinData.InitBuildInfo();
            // TODO BuiltInfo无法在Manager里面直接读取,所以只能放在这里,后续考虑将manager和component合并成一个类,然后参考ET Entity的方式重写Component
            GameEntry.Resource.InitBuildInfo();

            // 语言配置：设置当前使用的语言，如果不设置，则默认使用操作系统语言
            InitLanguageSettings();

            // 声音配置：根据用户配置数据，设置即将使用的声音选项
            InitSoundSettings();

            procedureOwner.SetData<VarString>("PackageName", "DefaultPackage");
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            // 运行一帧即切换到 Splash 展示流程
            ChangeState<ProcedureSplash>(procedureOwner);
        }

        private void InitLanguageSettings()
        {
            Language language;
            if (GameEntry.Base.ResourceMode == ResourceMode.EditorSimulateMode &&
                GameEntry.Base.EditorLanguage != Language.Unspecified)
                // 编辑器资源模式直接使用 Inspector 上设置的语言
            {
                language = GameEntry.Base.EditorLanguage;
                GameEntry.Localization.InitBuiltinLocalization(language);
                Log.Info("Init language settings complete in editor mode, current language is '{0}'.", language.ToString());
                return;
            }

            language = GameEntry.Localization.Language;
            if (GameEntry.Setting.HasSetting(Constant.Setting.Language))
                try
                {
                    var languageString = GameEntry.Setting.GetString(Constant.Setting.Language);
                    language = (Language)Enum.Parse(typeof(Language), languageString);
                }
                catch
                {
                }

            if (!LocalizationSupportedLanguage.Languages.Contains(language))
            {
                // 若是暂不支持的语言，则使用英语
                language = Language.English;

                GameEntry.Setting.SetString(Constant.Setting.Language, language.ToString());
                GameEntry.Setting.Save();
            }

            GameEntry.Localization.InitBuiltinLocalization(language);
            Log.Info("Init language settings complete, current language is '{0}'.", language.ToString());
        }


        private void InitSoundSettings()
        {
            GameEntry.Sound.Mute("Music", GameEntry.Setting.GetBool(Constant.Setting.MusicMuted, false));
            GameEntry.Sound.SetVolume("Music", GameEntry.Setting.GetFloat(Constant.Setting.MusicVolume, 0.3f));
            GameEntry.Sound.Mute("Sound", GameEntry.Setting.GetBool(Constant.Setting.SoundMuted, false));
            GameEntry.Sound.SetVolume("Sound", GameEntry.Setting.GetFloat(Constant.Setting.SoundVolume, 1f));
            GameEntry.Sound.Mute("UISound", GameEntry.Setting.GetBool(Constant.Setting.UISoundMuted, false));
            GameEntry.Sound.SetVolume("UISound", GameEntry.Setting.GetFloat(Constant.Setting.UISoundVolume, 1f));
            Log.Info("Init sound settings complete.");
        }
    }
}