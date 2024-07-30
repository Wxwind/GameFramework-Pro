using System;
using Cysharp.Threading.Tasks;
using UnityGameFramework.Runtime;

namespace GameMain
{
    public static class EntityExtension
    {
        // 关于 EntityId 的约定：
        // 0 为无效
        // 正值用于和服务器通信的实体（如玩家角色、NPC、怪等，服务器只产生正值）
        // 负值用于本地生成的临时实体（如特效、FakeObject等）
        private static int s_SerialId;

        public static Entity GetGameEntity(this EntityComponent entityComponent, int entityId)
        {
            var entity = entityComponent.GetEntity(entityId);
            if (entity == null) return null;

            return (Entity)entity.Logic;
        }

        public static void HideEntity(this EntityComponent entityComponent, Entity entity)
        {
            entityComponent.HideEntity(entity.Entity);
        }

        public static void AttachEntity(this EntityComponent entityComponent, Entity entity, int ownerId,
            string parentTransformPath = null, object userData = null)
        {
            entityComponent.AttachEntity(entity.Entity, ownerId, parentTransformPath, userData);
        }

        public static async UniTask<MyAircraft> ShowMyAircraft(this EntityComponent entityComponent,
            MyAircraftData data)
        {
            await entityComponent.ShowEntity(typeof(MyAircraft), "Aircraft", Constant.AssetPriority.MyAircraftAsset,
                data) as MyAircraft;
        }

        public static async UniTask ShowAircraft(this EntityComponent entityComponent, AircraftData data)
        {
            await entityComponent.ShowEntity(typeof(Aircraft), "Aircraft", Constant.AssetPriority.AircraftAsset, data);
        }

        public static async UniTask ShowThruster(this EntityComponent entityComponent, ThrusterData data)
        {
            await entityComponent.ShowEntity(typeof(Thruster), "Thruster", Constant.AssetPriority.ThrusterAsset, data);
        }

        public static async UniTask ShowWeapon(this EntityComponent entityComponent, WeaponData data)
        {
            await entityComponent.ShowEntity(typeof(Weapon), "Weapon", Constant.AssetPriority.WeaponAsset, data);
        }

        public static async UniTask ShowArmor(this EntityComponent entityComponent, ArmorData data)
        {
            await entityComponent.ShowEntity(typeof(Armor), "Armor", Constant.AssetPriority.ArmorAsset, data);
        }

        public static async UniTask ShowBullet(this EntityComponent entityCompoennt, BulletData data)
        {
            await entityCompoennt.ShowEntity(typeof(Bullet), "Bullet", Constant.AssetPriority.BulletAsset, data);
        }

        public static async UniTask ShowAsteroid(this EntityComponent entityCompoennt, AsteroidData data)
        {
            await entityCompoennt.ShowEntity(typeof(Asteroid), "Asteroid", Constant.AssetPriority.AsteroiAsset, data);
        }

        public static async UniTask ShowEffect(this EntityComponent entityComponent, EffectData data)
        {
            await entityComponent.ShowEntity(typeof(Effect), "Effect", Constant.AssetPriority.EffectAsset, data);
        }

        private static async UniTask ShowEntity(this EntityComponent entityComponent, Type logicType,
            string entityGroup,
            int priority, EntityData data)
        {
            if (data == null)
            {
                Log.Warning("Data is invalid.");
                return;
            }

            var tbEntity = GameEntry.LubanConfig.Tables.TbEntity;
            var drEntity = tbEntity.GetOrDefault(data.TypeId);

            if (drEntity == null)
            {
                Log.Warning("Can not load entity id '{0}' from data table.", data.TypeId.ToString());
                return;
            }

            await entityComponent.ShowEntity(data.Id, logicType, AssetUtility.GetEntityAsset(drEntity.AssetName),
                entityGroup,
                priority, data);
        }

        public static int GenerateSerialId(this EntityComponent entityComponent)
        {
            return --s_SerialId;
        }
    }
}