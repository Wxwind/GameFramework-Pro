using System;
using UnityEngine;

namespace Game
{
    [Serializable]
    public abstract class AccessoryObjectData : EntityData
    {
        [SerializeField] private int m_OwnerId;

        [SerializeField] private CampType m_OwnerCamp = CampType.Unknown;

        public AccessoryObjectData(int entityId, int typeId, int ownerId, CampType ownerCamp)
            : base(entityId, typeId)
        {
            m_OwnerId = ownerId;
            m_OwnerCamp = ownerCamp;
        }

        /// <summary>
        ///     拥有者编号。
        /// </summary>
        public int OwnerId => m_OwnerId;

        /// <summary>
        ///     拥有者阵营。
        /// </summary>
        public CampType OwnerCamp => m_OwnerCamp;
    }
}