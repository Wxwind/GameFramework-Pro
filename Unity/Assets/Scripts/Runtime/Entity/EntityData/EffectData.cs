using System;
using UnityEngine;

namespace GameMain
{
    [Serializable]
    public class EffectData : EntityData
    {
        [SerializeField] private float m_KeepTime;

        public EffectData(int entityId, int typeId)
            : base(entityId, typeId)
        {
            m_KeepTime = 3f;
        }

        public float KeepTime => m_KeepTime;
    }
}