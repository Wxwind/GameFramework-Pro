//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using UnityEngine;

namespace StarForce
{
    [Serializable]
    public class AsteroidData : TargetableObjectData
    {
        [SerializeField] private int m_MaxHP = 0;

        [SerializeField] private int m_Attack = 0;

        [SerializeField] private float m_Speed = 0f;

        [SerializeField] private float m_AngularSpeed = 0f;

        [SerializeField] private int m_DeadEffectId = 0;

        [SerializeField] private int m_DeadSoundId = 0;

        public AsteroidData(int entityId, int typeId)
            : base(entityId, typeId, CampType.Neutral)
        {
            var tbAsteroid = GameEntry.LubanConfig.Tables.TbAsteroid;
            var drAsteroid = tbAsteroid.GetOrDefault(TypeId);
            if (drAsteroid == null)
            {
                return;
            }

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