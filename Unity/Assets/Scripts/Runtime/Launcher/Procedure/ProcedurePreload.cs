using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GFPro;
using UnityEngine;
using ProcedureOwner = GFPro.IFsm<GFPro.Procedure.ProcedureComponent>;

namespace Game
{
    public class ProcedurePreload : ProcedureBase
    {
        private readonly Dictionary<string, bool> m_LoadedFlag = new();


        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_LoadedFlag.Clear();
            PreloadResources();
        }


        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            foreach (var loadedFlag in m_LoadedFlag)
                if (!loadedFlag.Value)
                    return;

            procedureOwner.SetData<VarInt32>("NextSceneId", GameEntry.Config.GetInt("Scene.Menu"));
            Log.Info($"Change to ProcedureChangeScene, SceneId: {GameEntry.Config.GetInt("Scene.Menu")}");
            UILaunchMgr.DestroySelf();
            ChangeState<ProcedureChangeScene>(procedureOwner);
        }

        private void PreloadResources()
        {
            // Preload configs
            LoadConfig("DefaultConfig").Forget();

            // Preload data tables
            LoadLubanDataTable().Forget();

            // Preload dictionaries
            LoadL10N().Forget();

            // Preload fonts
            LoadFont("MainFont").Forget();
        }

        private async UniTaskVoid LoadConfig(string configName)
        {
            try
            {
                m_LoadedFlag.Add(configName, false);
                await GameEntry.Config.ReadData(configName);
                m_LoadedFlag[configName] = true;
                Log.Info($"Load config '{configName}' OK.");
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        private async UniTaskVoid LoadLubanDataTable()
        {
            try
            {
                m_LoadedFlag.Add("LubanDataTable", false);
                await GameEntry.LubanDataTable.LoadAsync();
                m_LoadedFlag["LubanDataTable"] = true;
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        private async UniTaskVoid LoadL10N()
        {
            var dictionaryAssetName = GameEntry.Localization.Language.ToString();
            try
            {
                m_LoadedFlag.Add(dictionaryAssetName, false);
                await GameEntry.Localization.InitAsync();
                m_LoadedFlag[dictionaryAssetName] = true;
                Log.Info($"Load dictionary '{dictionaryAssetName}' OK.");
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        private async UniTaskVoid LoadFont(string fontName)
        {
            try
            {
                m_LoadedFlag.Add(Utility.Text.Format("Font.{0}", fontName), false);
                var asset = await GameEntry.Resource.LoadAssetAsync<Font>(fontName
                );
                m_LoadedFlag[Utility.Text.Format("Font.{0}", fontName)] = true;
                UGuiForm.SetMainFont(asset);
                Log.Info($"Load font '{fontName}' OK.");
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }
    }
}