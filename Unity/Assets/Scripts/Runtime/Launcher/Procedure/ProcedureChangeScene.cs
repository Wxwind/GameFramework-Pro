using Cysharp.Threading.Tasks;
using GFPro;
using UnityEngine.SceneManagement;
using ProcedureOwner = GFPro.IFsm<GFPro.Procedure.ProcedureComponent>;

namespace Game
{
    public class ProcedureChangeScene : ProcedureBase
    {
        private const int MenuSceneId = 1;
        private       int m_BackgroundMusicId;

        private bool m_ChangeToMenu;
        private bool m_IsChangeSceneComplete;


        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_IsChangeSceneComplete = false;


            // 停止所有声音
            GameEntry.Sound.StopAllLoadingSounds();
            GameEntry.Sound.StopAllLoadedSounds();

            // 隐藏所有实体
            GameEntry.Entity.HideAllLoadingEntities();
            GameEntry.Entity.HideAllLoadedEntities();

            // 卸载所有场景
            var loadedSceneAssetNames = GameEntry.Scene.GetLoadedSceneAssetNames();
            for (var i = 0; i < loadedSceneAssetNames.Length; i++)
                GameEntry.Scene.UnloadScene(loadedSceneAssetNames[i]).Forget();

            // 还原游戏速度
            GameEntry.Base.ResetNormalGameSpeed();

            int sceneId = procedureOwner.GetData<VarInt32>("NextSceneId");
            m_ChangeToMenu = sceneId == MenuSceneId;
            var tbScene = GameEntry.LubanDataTable.Tables.TbScene;
            var drScene = tbScene.GetOrDefault(sceneId);
            if (drScene == null)
            {
                Log.Warning($"Can not load scene '{sceneId.ToString()}' from data table.");
                return;
            }

            Log.Info($"change scene to id:${sceneId}, m_ChangeToMenu={m_ChangeToMenu}");
            LoadScene(drScene.AssetName).Forget();
            m_BackgroundMusicId = drScene.BackgroundMusicId;
        }

        private async UniTask LoadScene(string sceneName, uint priority = 0)
        {
            await GameEntry.Scene.LoadSceneAsync(sceneName, null, LoadSceneMode.Additive,
                false, priority, OnLoadSceneUpdate);
            Log.Info($"Load scene '{sceneName}' OK.");

            if (m_BackgroundMusicId > 0) GameEntry.Sound.PlayMusic(m_BackgroundMusicId);

            m_IsChangeSceneComplete = true;
            return;

            void OnLoadSceneUpdate(float progress)
            {
                Log.Info($"Load scene '{sceneName}' update, progress '{progress}'.");
            }
        }


        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!m_IsChangeSceneComplete) return;

            if (m_ChangeToMenu)
                ChangeState<ProcedureMenu>(procedureOwner);
            else
                ChangeState<ProcedureMain>(procedureOwner);
        }
    }
}