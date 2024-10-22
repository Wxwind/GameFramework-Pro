﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GFPro;
using UnityEngine;

namespace Game
{
    /// <summary>
    ///     AI 工具类。
    /// </summary>
    public static class AIUtility
    {
        private static readonly Dictionary<CampPair, RelationType> s_CampPairToRelation = new();

        private static readonly Dictionary<KeyValuePair<CampType, RelationType>, CampType[]> s_CampAndRelationToCamps =
            new();

        static AIUtility()
        {
            s_CampPairToRelation.Add(new CampPair(CampType.Player, CampType.Player), RelationType.Friendly);
            s_CampPairToRelation.Add(new CampPair(CampType.Player, CampType.Enemy), RelationType.Hostile);
            s_CampPairToRelation.Add(new CampPair(CampType.Player, CampType.Neutral), RelationType.Neutral);
            s_CampPairToRelation.Add(new CampPair(CampType.Player, CampType.Player2), RelationType.Hostile);
            s_CampPairToRelation.Add(new CampPair(CampType.Player, CampType.Enemy2), RelationType.Hostile);
            s_CampPairToRelation.Add(new CampPair(CampType.Player, CampType.Neutral2), RelationType.Neutral);

            s_CampPairToRelation.Add(new CampPair(CampType.Enemy, CampType.Enemy), RelationType.Friendly);
            s_CampPairToRelation.Add(new CampPair(CampType.Enemy, CampType.Neutral), RelationType.Neutral);
            s_CampPairToRelation.Add(new CampPair(CampType.Enemy, CampType.Player2), RelationType.Hostile);
            s_CampPairToRelation.Add(new CampPair(CampType.Enemy, CampType.Enemy2), RelationType.Hostile);
            s_CampPairToRelation.Add(new CampPair(CampType.Enemy, CampType.Neutral2), RelationType.Neutral);

            s_CampPairToRelation.Add(new CampPair(CampType.Neutral, CampType.Neutral), RelationType.Neutral);
            s_CampPairToRelation.Add(new CampPair(CampType.Neutral, CampType.Player2), RelationType.Neutral);
            s_CampPairToRelation.Add(new CampPair(CampType.Neutral, CampType.Enemy2), RelationType.Neutral);
            s_CampPairToRelation.Add(new CampPair(CampType.Neutral, CampType.Neutral2), RelationType.Hostile);

            s_CampPairToRelation.Add(new CampPair(CampType.Player2, CampType.Player2), RelationType.Friendly);
            s_CampPairToRelation.Add(new CampPair(CampType.Player2, CampType.Enemy2), RelationType.Hostile);
            s_CampPairToRelation.Add(new CampPair(CampType.Player2, CampType.Neutral2), RelationType.Neutral);

            s_CampPairToRelation.Add(new CampPair(CampType.Enemy2, CampType.Enemy2), RelationType.Friendly);
            s_CampPairToRelation.Add(new CampPair(CampType.Enemy2, CampType.Neutral2), RelationType.Neutral);

            s_CampPairToRelation.Add(new CampPair(CampType.Neutral2, CampType.Neutral2), RelationType.Neutral);
        }

        /// <summary>
        ///     获取两个阵营之间的关系。
        /// </summary>
        /// <param name="first">阵营一。</param>
        /// <param name="second">阵营二。</param>
        /// <returns>阵营间关系。</returns>
        public static RelationType GetRelation(CampType first, CampType second)
        {
            if (first > second)
            {
                var temp = first;
                first = second;
                second = temp;
            }

            RelationType relationType;
            if (s_CampPairToRelation.TryGetValue(new CampPair(first, second), out relationType)) return relationType;

            Log.Warning("Unknown relation between '{0}' and '{1}'.", first.ToString(), second.ToString());
            return RelationType.Unknown;
        }

        /// <summary>
        ///     获取和指定具有特定关系的所有阵营。
        /// </summary>
        /// <param name="camp">指定阵营。</param>
        /// <param name="relation">关系。</param>
        /// <returns>满足条件的阵营数组。</returns>
        public static CampType[] GetCamps(CampType camp, RelationType relation)
        {
            var key = new KeyValuePair<CampType, RelationType>(camp, relation);
            CampType[] result = null;
            if (s_CampAndRelationToCamps.TryGetValue(key, out result)) return result;

            // TODO: GC Alloc
            var camps = new List<CampType>();
            var campTypes = Enum.GetValues(typeof(CampType));
            for (var i = 0; i < campTypes.Length; i++)
            {
                var campType = (CampType)campTypes.GetValue(i);
                if (GetRelation(camp, campType) == relation) camps.Add(campType);
            }

            // TODO: GC Alloc
            result = camps.ToArray();
            s_CampAndRelationToCamps[key] = result;

            return result;
        }

        /// <summary>
        ///     获取实体间的距离。
        /// </summary>
        /// <returns>实体间的距离。</returns>
        public static float GetDistance(Entity fromEntity, Entity toEntity)
        {
            var fromTransform = fromEntity.CachedTransform;
            var toTransform = toEntity.CachedTransform;
            return (toTransform.position - fromTransform.position).magnitude;
        }

        public static void PerformCollision(TargetableObject entity, Entity other)
        {
            if (entity == null || other == null) return;

            var target = other as TargetableObject;
            if (target != null)
            {
                var entityImpactData = entity.GetImpactData();
                var targetImpactData = target.GetImpactData();
                if (GetRelation(entityImpactData.Camp, targetImpactData.Camp) == RelationType.Friendly) return;

                var entityDamageHP = CalcDamageHP(targetImpactData.Attack, entityImpactData.Defense);
                var targetDamageHP = CalcDamageHP(entityImpactData.Attack, targetImpactData.Defense);

                var delta = Mathf.Min(entityImpactData.HP - entityDamageHP, targetImpactData.HP - targetDamageHP);
                if (delta > 0)
                {
                    entityDamageHP += delta;
                    targetDamageHP += delta;
                }

                entity.ApplyDamage(target, entityDamageHP);
                target.ApplyDamage(entity, targetDamageHP);
                return;
            }

            var bullet = other as Bullet;
            if (bullet != null)
            {
                var entityImpactData = entity.GetImpactData();
                var bulletImpactData = bullet.GetImpactData();
                if (GetRelation(entityImpactData.Camp, bulletImpactData.Camp) == RelationType.Friendly) return;

                var entityDamageHP = CalcDamageHP(bulletImpactData.Attack, entityImpactData.Defense);

                entity.ApplyDamage(bullet, entityDamageHP);
                GameEntry.Entity.HideEntity(bullet);
            }
        }

        private static int CalcDamageHP(int attack, int defense)
        {
            if (attack <= 0) return 0;

            if (defense < 0) defense = 0;

            return attack * attack / (attack + defense);
        }

        [StructLayout(LayoutKind.Auto)]
        private struct CampPair
        {
            public CampPair(CampType first, CampType second)
            {
                First = first;
                Second = second;
            }

            public CampType First { get; }

            public CampType Second { get; }
        }
    }
}