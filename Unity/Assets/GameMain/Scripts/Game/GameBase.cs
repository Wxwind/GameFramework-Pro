using Cysharp.Threading.Tasks;
using GameFramework.Event;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace GameMain
{
    public abstract class GameBase
    {
        private MyAircraft m_MyAircraft;

        public abstract GameMode GameMode { get; }

        protected ScrollableBackground SceneBackground { get; private set; }

        public bool GameOver { get; protected set; }

        public virtual async UniTaskVoid Initialize()
        {
            GameEntry.Event.Subscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);
            GameEntry.Event.Subscribe(ShowEntityFailureEventArgs.EventId, OnShowEntityFailure);

            SceneBackground = Object.FindObjectOfType<ScrollableBackground>();
            if (SceneBackground == null)
            {
                Log.Warning("Can not find scene background.");
                return;
            }

            SceneBackground.VisibleBoundary.gameObject.GetOrAddComponent<HideByBoundary>();
            var entity = GameEntry.Entity.ShowMyAircraft(new MyAircraftData(GameEntry.Entity.GenerateSerialId(), 10000)
            {
                Name = "My Aircraft",
                Position = Vector3.zero
            });
            m_MyAircraft = (MyAircraft)entity.Logic;

            GameOver = false;
            m_MyAircraft = null;
        }

        public virtual void Shutdown()
        {
            GameEntry.Event.Unsubscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);
            GameEntry.Event.Unsubscribe(ShowEntityFailureEventArgs.EventId, OnShowEntityFailure);
        }

        public virtual void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (m_MyAircraft != null && m_MyAircraft.IsDead)
            {
                GameOver = true;
            }
        }

        protected virtual void OnShowEntitySuccess(object sender, GameEventArgs e)
        {
            var ne = (ShowEntitySuccessEventArgs)e;
            if (ne.EntityLogicType == typeof(MyAircraft)) ;
        }

        protected virtual void OnShowEntityFailure(object sender, GameEventArgs e)
        {
            var ne = (ShowEntityFailureEventArgs)e;
            Log.Warning("Show entity failure with error message '{0}'.", ne.ErrorMessage);
        }
    }
}