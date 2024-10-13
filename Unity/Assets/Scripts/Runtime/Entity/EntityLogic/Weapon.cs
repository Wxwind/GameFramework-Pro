using Cysharp.Threading.Tasks;
using GFPro;
using GFPro.Entity;
using UnityEngine;

namespace Game
{
    /// <summary>
    ///     武器类。
    /// </summary>
    public class Weapon : Entity
    {
        private const string AttachPoint = "Weapon Point";

        [SerializeField] private WeaponData m_WeaponData;

        private float m_NextAttackTime;


        protected override void OnShow(object userData)
        {
            base.OnShow(userData);

            m_WeaponData = userData as WeaponData;
            if (m_WeaponData == null)
            {
                Log.Error("Weapon data is invalid.");
                return;
            }

            GameEntry.Entity.AttachEntity(Entity.Id, m_WeaponData.OwnerId, AttachPoint);
        }


        protected override void OnAttachTo(EntityLogic parentEntity, Transform parentTransform, object userData)
        {
            base.OnAttachTo(parentEntity, parentTransform, userData);

            Name = Utility.Text.Format("Weapon of {0}", parentEntity.Name);
            CachedTransform.localPosition = Vector3.zero;
        }

        public void TryAttack()
        {
            if (Time.time < m_NextAttackTime) return;

            m_NextAttackTime = Time.time + m_WeaponData.AttackInterval;
            GameEntry.Entity.ShowBullet(new BulletData(GameEntry.Entity.GenerateSerialId(), m_WeaponData.BulletId,
                m_WeaponData.OwnerId, m_WeaponData.OwnerCamp, m_WeaponData.Attack, m_WeaponData.BulletSpeed)
            {
                Position = CachedTransform.position
            }).Forget();
            GameEntry.Sound.PlaySound(m_WeaponData.BulletSoundId);
        }
    }
}