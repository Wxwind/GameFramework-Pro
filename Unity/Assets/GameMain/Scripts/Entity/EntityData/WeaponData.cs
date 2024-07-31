using System;
using UnityEngine;

namespace GameMain
{
    [Serializable]
    public class WeaponData : AccessoryObjectData
    {
        [SerializeField] private int m_Attack;

        [SerializeField] private float m_AttackInterval;

        [SerializeField] private int m_BulletId;

        [SerializeField] private float m_BulletSpeed;

        [SerializeField] private int m_BulletSoundId;

        public WeaponData(int entityId, int typeId, int ownerId, CampType ownerCamp)
            : base(entityId, typeId, ownerId, ownerCamp)
        {
            var tbWeapon = GameEntry.LubanConfig.Tables.TbWeapon;
            cfg.StarForce.Weapon drWeapon = null;
            if (TypeId != 0) drWeapon = tbWeapon.GetOrDefault(TypeId);
            if (drWeapon == null) return;

            m_Attack = drWeapon.Attack;
            m_AttackInterval = drWeapon.AttackInterval;
            m_BulletId = drWeapon.BulletId;
            m_BulletSpeed = drWeapon.BulletSpeed;
            m_BulletSoundId = drWeapon.BulletSoundId;
        }

        /// <summary>
        ///     攻击力。
        /// </summary>
        public int Attack => m_Attack;

        /// <summary>
        ///     攻击间隔。
        /// </summary>
        public float AttackInterval => m_AttackInterval;

        /// <summary>
        ///     子弹编号。
        /// </summary>
        public int BulletId => m_BulletId;

        /// <summary>
        ///     子弹速度。
        /// </summary>
        public float BulletSpeed => m_BulletSpeed;

        /// <summary>
        ///     子弹声音编号。
        /// </summary>
        public int BulletSoundId => m_BulletSoundId;
    }
}