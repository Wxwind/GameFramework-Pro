using System;
using UnityEngine;

namespace GameMain
{
    [Serializable]
    public class AsteroidData : TargetableObjectData
    {
        [SerializeField] private int m_MaxHP;

        [SerializeField] private int m_Attack;

        [SerializeField] private float m_Speed;

        [SerializeField] private float m_AngularSpeed;

        [SerializeField] private int m_DeadEffectId;

        [SerializeField] private int m_DeadSoundId;

        public AsteroidData(int entityId, int typeId)
            : base(entityId, typeId, CampType.Neutral)
        {
            var tbAsteroid = GameEntry.LubanConfig.Tables.TbAsteroid;
            var drAsteroid = tbAsteroid.GetOrDefault(TypeId);
            if (drAsteroid == null) return;

            HP = m_MaxHP = drAsteroid.MaxHP;
            m_Attack = drAsteroid.Attack;
            m_Speed = drAsteroid.Speed;
            m_AngularSpeed = drAsteroid.AngularSpeed;
            m_DeadEffectId = drAsteroid.DeadEffectId;
            m_DeadSoundId = drAsteroid.DeadSoundId;
        }

        public override int MaxHP => m_MaxHP;

        public int Attack => m_Attack;

        public float Speed => m_Speed;

        public float AngularSpeed => m_AngularSpeed;

        public int DeadEffectId => m_DeadEffectId;

        public int DeadSoundId => m_DeadSoundId;
    }
}