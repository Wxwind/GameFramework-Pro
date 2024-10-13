using GFPro;
using UnityEngine;

namespace Game
{
    /// <summary>
    ///     可作为目标的实体类。
    /// </summary>
    public abstract class TargetableObject : GameEntity
    {
        [SerializeField] private TargetableObjectData m_TargetableObjectData;

        public bool IsDead => m_TargetableObjectData.HP <= 0;

        private void OnTriggerEnter(Collider other)
        {
            var entity = other.gameObject.GetComponent<GameEntity>();
            if (entity == null) return;

            if (entity is TargetableObject && entity.Id >= Id)
                // 碰撞事件由 Id 小的一方处理，避免重复处理
                return;

            AIUtility.PerformCollision(this, entity);
        }

        public abstract ImpactData GetImpactData();

        public void ApplyDamage(GameEntity attacker, int damageHP)
        {
            var fromHPRatio = m_TargetableObjectData.HPRatio;
            m_TargetableObjectData.HP -= damageHP;
            var toHPRatio = m_TargetableObjectData.HPRatio;
            if (fromHPRatio > toHPRatio) GameEntry.HPBar.ShowHPBar(this, fromHPRatio, toHPRatio);

            if (m_TargetableObjectData.HP <= 0) OnDead(attacker);
        }


        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            gameObject.SetLayerRecursively(Constant.Layer.TargetableObjectLayerId);
        }


        protected override void OnShow(object userData)
        {
            base.OnShow(userData);

            m_TargetableObjectData = userData as TargetableObjectData;
            if (m_TargetableObjectData == null) Log.Error("Targetable object data is invalid.");
        }

        protected virtual void OnDead(GameEntity attacker)
        {
            GameEntry.Entity.HideEntity(this);
        }
    }
}