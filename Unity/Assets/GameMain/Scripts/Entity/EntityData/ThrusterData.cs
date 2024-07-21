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
    public class ThrusterData : AccessoryObjectData
    {
        [SerializeField] private float m_Speed = 0f;

        public ThrusterData(int entityId, int typeId, int ownerId, CampType ownerCamp)
            : base(entityId, typeId, ownerId, ownerCamp)
        {
            var tbThruster = GameEntry.LubanConfig.Tables.TbThruster;
            var drThruster = tbThruster.GetOrDefault(TypeId);
            if (drThruster == null)
            {
                return;
            }

            m_Speed = drThruster.Speed;
        }

        /// <summary>
        /// 速度。
        /// </summary>
        public float Speed => m_Speed;
    }
}