using UnityEngine;
using UnityGameFramework.Runtime;

namespace GameMain
{
    /// <summary>
    ///     子弹类。
    /// </summary>
    public class Bullet : Entity
    {
        [SerializeField] private BulletData m_BulletData;

        public ImpactData GetImpactData()
        {
            return new ImpactData(m_BulletData.OwnerCamp, 0, m_BulletData.Attack, 0);
        }


        protected override void OnShow(object userData)
        {
            base.OnShow(userData);

            m_BulletData = userData as BulletData;
            if (m_BulletData == null) Log.Error("Bullet data is invalid.");
        }


        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            CachedTransform.Translate(Vector3.forward * m_BulletData.Speed * elapseSeconds, Space.World);
        }
    }
}