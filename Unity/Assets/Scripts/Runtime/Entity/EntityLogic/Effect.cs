﻿using GFPro;
using UnityEngine;

namespace Game
{
    /// <summary>
    ///     特效类。
    /// </summary>
    public class Effect : Entity
    {
        [SerializeField] private EffectData m_EffectData;

        private float m_ElapseSeconds;


        protected override void OnShow(object userData)
        {
            base.OnShow(userData);

            m_EffectData = userData as EffectData;
            if (m_EffectData == null)
            {
                Log.Error("Effect data is invalid.");
                return;
            }

            m_ElapseSeconds = 0f;
        }


        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            m_ElapseSeconds += elapseSeconds;
            if (m_ElapseSeconds >= m_EffectData.KeepTime) GameEntry.Entity.HideEntity(this);
        }
    }
}