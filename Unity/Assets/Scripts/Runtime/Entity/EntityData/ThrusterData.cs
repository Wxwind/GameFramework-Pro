using System;
using UnityEngine;

namespace GameMain
{
    [Serializable]
    public class ThrusterData : AccessoryObjectData
    {
        [SerializeField] private float m_Speed;

        public ThrusterData(int entityId, int typeId, int ownerId, CampType ownerCamp)
            : base(entityId, typeId, ownerId, ownerCamp)
        {
            var tbThruster = GameEntry.LubanConfig.Tables.TbThruster;
            var drThruster = tbThruster.GetOrDefault(TypeId);
            if (drThruster == null) return;

            m_Speed = drThruster.Speed;
        }

        /// <summary>
        ///     速度。
        /// </summary>
        public float Speed => m_Speed;
    }
}