using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.Event;
using GameFramework.Resource;
using UnityEngine;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace GameMain
{
    public class ProcedurePreload : ProcedureBase
    {
        private readonly Dictionary<string, bool> m_LoadedFlag = new();

        public override bool UseNativeDialog => true;

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
            Log.Info("热更流程完毕，准备进入场景切换流程");
            UILaunchMgr.DestroySelf();
            ChangeState<ProcedureChangeScene>(procedureOwner);
        }

        private void PreloadResources()
        {
            // TODO 全部使用 yooasset 加载
            // Preload configs
            LoadConfig("DefaultConfig");

            // Preload data tables
            // TODO 在这里使用异步加载 LubanConfig

            // Preload dictionaries
            LoadL10N().Forget();

            // Preload fonts
            LoadFont("MainFont").Forget();
        }

        private async UniTaskVoid LoadConfig(string configName)
        {
            try
            {
                var configAssetName = AssetUtility.GetConfigAsset(configName, false);
                m_LoadedFlag.Add(configAssetName, false);
                await GameEntry.Config.ReadData(configAssetName);
                m_LoadedFlag[configAssetName] = true;
                Log.Info("Load config '{0}' OK.", configAssetName);
            }
            catch (Exception e)
            {
                Log.Error("Can not load config  '{0}' with error message '{2}'.", configName,
                    e.Message);
                throw;
            }
        }


        private async UniTaskVoid LoadL10N()
        {
            var dictionaryAssetName = AssetUtility.GetDictionaryAsset();
            try
            {
                m_LoadedFlag.Add(dictionaryAssetName, false);
                await GameEntry.Localization.ReadData(dictionaryAssetName);
                m_LoadedFlag[dictionaryAssetName] = true;
                Log.Info("Load dictionary '{0}' OK.", dictionaryAssetName);
            }
            catch (Exception e)
            {
                Log.Error("Can not load dictionary '{0}' with error message '{2}'.", dictionaryAssetName,
                    e.Message);
            }
        }

        private async UniTaskVoid LoadFont(string fontName)
        {
            m_LoadedFlag.Add(Utility.Text.Format("Font.{0}", fontName), false);
            var asset = await GameEntry.Resource.LoadAssetAsync<Font>(fontName, "", null
            );

            if (asset == null)
            {
                var errorMsg = Utility.Text.Format("Can not load asset '{0}' because :'{1}'.", fontName,
                    "asset is not exist");
                Log.Error("Can not load font '{0}' with error message '{2}'.", fontName,
                    errorMsg);
            }
            else
            {
                m_LoadedFlag[Utility.Text.Format("Font.{0}", fontName)] = true;
                UGuiForm.SetMainFont((Font)asset);
                Log.Info("Load font '{0}' OK.", fontName);
            }
        }
    }
}