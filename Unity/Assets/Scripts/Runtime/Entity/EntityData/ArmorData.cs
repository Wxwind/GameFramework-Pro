using System;
using UnityEngine;

namespace Game
{
    [Serializable]
    public class ArmorData : AccessoryObjectData
    {
        [SerializeField] private int m_MaxHP;

        [SerializeField] private int m_Defense;

        public ArmorData(int entityId, int typeId, int ownerId, CampType ownerCamp)
            : base(entityId, typeId, ownerId, ownerCamp)
        {
            var tbArmor = GameEntry.LubanConfig.Tables.TbArmor;
            var drArmor = tbArmor.GetOrDefault(TypeId);
            if (drArmor == null) return;

            m_MaxHP = drArmor.MaxHP;
            m_Defense = drArmor.Defense;
        }

        /// <summary>
        ///     最大生命。
        /// </summary>
        public int MaxHP => m_MaxHP;

        /// <summary>
        ///     防御力。
        /// </summary>
        public int Defense => m_Defense;
    }
}