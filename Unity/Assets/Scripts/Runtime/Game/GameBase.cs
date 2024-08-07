using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Game
{
    public abstract class GameBase
    {
        private MyAircraft m_MyAircraft;

        public abstract GameMode GameMode { get; }

        protected ScrollableBackground SceneBackground { get; private set; }

        public bool GameOver { get; protected set; }

        public virtual async UniTaskVoid Initialize()
        {
            GameOver = false;
            m_MyAircraft = null;
            SceneBackground = Object.FindObjectOfType<ScrollableBackground>();
            if (SceneBackground == null)
            {
                Log.Warning("Can not find scene background.");
                return;
            }

            SceneBackground.VisibleBoundary.gameObject.GetOrAddComponent<HideByBoundary>();
            var entity = await GameEntry.Entity.ShowMyAircraft(new MyAircraftData(GameEntry.Entity.GenerateSerialId(), 10000)
            {
                Name = "My Aircraft",
                Position = Vector3.zero
            });
            m_MyAircraft = entity;
        }

        public virtual void Shutdown()
        {
        }

        public virtual void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (m_MyAircraft != null && m_MyAircraft.IsDead)
            {
                GameOver = true;
            }
        }
    }
}