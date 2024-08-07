using System;
using Cysharp.Threading.Tasks;
using UnityGameFramework.Runtime;

namespace Game
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
            var entity = await entityComponent.ShowEntity(typeof(MyAircraft), "Aircraft",
                data);
            return entity.Logic as MyAircraft;
        }

        public static async UniTask<Aircraft> ShowAircraft(this EntityComponent entityComponent, AircraftData data)
        {
            var entity = await entityComponent.ShowEntity(typeof(Aircraft), "Aircraft", data);
            return entity.Logic as Aircraft;
        }

        public static async UniTask<Thruster> ShowThruster(this EntityComponent entityComponent, ThrusterData data)
        {
            var entity = await entityComponent.ShowEntity(typeof(Thruster), "Thruster", data);
            return entity.Logic as Thruster;
        }

        public static async UniTask<Weapon> ShowWeapon(this EntityComponent entityComponent, WeaponData data)
        {
            var entity = await entityComponent.ShowEntity(typeof(Weapon), "Weapon", data);
            return entity.Logic as Weapon;
        }

        public static async UniTask<Armor> ShowArmor(this EntityComponent entityComponent, ArmorData data)
        {
            var entity = await entityComponent.ShowEntity(typeof(Armor), "Armor", data);
            return entity.Logic as Armor;
        }

        public static async UniTask<Bullet> ShowBullet(this EntityComponent entityCompoennt, BulletData data)
        {
            var entity = await entityCompoennt.ShowEntity(typeof(Bullet), "Bullet", data);
            return entity.Logic as Bullet;
        }

        public static async UniTask<Asteroid> ShowAsteroid(this EntityComponent entityCompoennt, AsteroidData data)
        {
            var entity = await entityCompoennt.ShowEntity(typeof(Asteroid), "Asteroid", data);
            return entity.Logic as Asteroid;
        }

        public static async UniTask<Effect> ShowEffect(this EntityComponent entityComponent, EffectData data)
        {
            var entity = await entityComponent.ShowEntity(typeof(Effect), "Effect", data);
            return entity.Logic as Effect;
        }

        private static async UniTask<UnityGameFramework.Runtime.Entity> ShowEntity(this EntityComponent entityComponent, Type logicType,
            string entityGroup,
            EntityData data)
        {
            if (data == null)
            {
                Log.Warning("Data is invalid.");
                return null;
            }

            var tbEntity = GameEntry.LubanConfig.Tables.TbEntity;
            var drEntity = tbEntity.GetOrDefault(data.TypeId);

            if (drEntity == null)
            {
                Log.Warning("Can not load entity id '{0}' from data table.", data.TypeId.ToString());
                return null;
            }

            return await entityComponent.ShowEntity(data.Id, logicType, drEntity.AssetName,
                entityGroup, data) as UnityGameFramework.Runtime.Entity;
        }

        public static int GenerateSerialId(this EntityComponent entityComponent)
        {
            return --s_SerialId;
        }
    }
}