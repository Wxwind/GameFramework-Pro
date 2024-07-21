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
    public class ArmorData : AccessoryObjectData
    {
        [SerializeField] private int m_MaxHP = 0;

        [SerializeField] private int m_Defense = 0;

        public ArmorData(int entityId, int typeId, int ownerId, CampType ownerCamp)
            : base(entityId, typeId, ownerId, ownerCamp)
        {
            var tbArmor = GameEntry.LubanConfig.Tables.TbArmor;
            var drArmor = tbArmor.GetOrDefault(TypeId);
            if (drArmor == null)
            {
                return;
            }

            m_MaxHP = drArmor.MaxHP;
            m_Defense = drArmor.Defense;
        }

        /// <summary>
        /// 最大生命。
        /// </summary>
        public int MaxHP => m_MaxHP;

        /// <summary>
        /// 防御力。
        /// </summary>
        public int Defense => m_Defense;
    }
}