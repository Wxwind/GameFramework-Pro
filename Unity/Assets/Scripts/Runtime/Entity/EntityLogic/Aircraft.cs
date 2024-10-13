using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GFPro;
using UnityEngine;

namespace Game
{
    /// <summary>
    ///     战机类。
    /// </summary>
    public abstract class Aircraft : TargetableObject
    {
        [SerializeField] protected Thruster m_Thruster;

        [SerializeField] protected List<Weapon> m_Weapons = new();

        [SerializeField] protected List<Armor> m_Armors = new();

        [SerializeField] private AircraftData m_AircraftData;


        protected override void OnShow(object userData)
        {
            base.OnShow(userData);

            m_AircraftData = userData as AircraftData;
            if (m_AircraftData == null)
            {
                Log.Error("Aircraft data is invalid.");
                return;
            }

            Name = $"Aircraft ({Id})";

            GameEntry.Entity.ShowThruster(m_AircraftData.GetThrusterData()).Forget();

            var weaponDatas = m_AircraftData.GetAllWeaponDatas();
            for (var i = 0; i < weaponDatas.Count; i++) GameEntry.Entity.ShowWeapon(weaponDatas[i]).Forget();

            var armorDatas = m_AircraftData.GetAllArmorDatas();
            for (var i = 0; i < armorDatas.Count; i++) GameEntry.Entity.ShowArmor(armorDatas[i]).Forget();
        }


        protected override void OnAttached(EntityLogic childEntity, Transform parentTransform, object userData)
        {
            base.OnAttached(childEntity, parentTransform, userData);

            if (childEntity is Thruster)
            {
                m_Thruster = (Thruster)childEntity;
                return;
            }

            if (childEntity is Weapon)
            {
                m_Weapons.Add((Weapon)childEntity);
                return;
            }

            if (childEntity is Armor) m_Armors.Add((Armor)childEntity);
        }


        protected override void OnDetached(EntityLogic childEntity, object userData)
        {
            base.OnDetached(childEntity, userData);

            if (childEntity is Thruster)
            {
                m_Thruster = null;
                return;
            }

            if (childEntity is Weapon)
            {
                m_Weapons.Remove((Weapon)childEntity);
                return;
            }

            if (childEntity is Armor) m_Armors.Remove((Armor)childEntity);
        }

        protected override void OnDead(GameEntity attacker)
        {
            base.OnDead(attacker);

            GameEntry.Entity.ShowEffect(new EffectData(GameEntry.Entity.GenerateSerialId(), m_AircraftData.DeadEffectId)
            {
                Position = CachedTransform.localPosition
            }).Forget();
            GameEntry.Sound.PlaySound(m_AircraftData.DeadSoundId);
        }

        public override ImpactData GetImpactData()
        {
            return new ImpactData(m_AircraftData.Camp, m_AircraftData.HP, 0, m_AircraftData.Defense);
        }
    }
}