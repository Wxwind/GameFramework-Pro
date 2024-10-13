using System;
using Cysharp.Threading.Tasks;
using GFPro;

namespace Game
{
    public static class EntityExtension
    {
        // 关于 EntityId 的约定：
        // 0 为无效
        // 正值用于和服务器通信的实体（如玩家角色、NPC、怪等，服务器只产生正值）
        // 负值用于本地生成的临时实体（如特效、FakeObject等）
        private static int s_SerialId;

        public static GameEntity GetGameEntity(this GFEntityComponent entityComponent, int entityId)
        {
            var entity = entityComponent.GetEntity(entityId);
            if (entity == null) return null;

            return (GameEntity)entity.Logic;
        }

        public static void HideEntity(this GFEntityComponent entityComponent, GameEntity gameEntity)
        {
            entityComponent.HideEntity(gameEntity.Entity);
        }

        public static void AttachEntity(this GFEntityComponent entityComponent, GameEntity gameEntity, int ownerId,
            string parentTransformPath = null, object userData = null)
        {
            entityComponent.AttachEntity(gameEntity.Entity.Id, ownerId, parentTransformPath, userData);
        }

        public static async UniTask<MyAircraft> ShowMyAircraft(this GFEntityComponent entityComponent,
            MyAircraftData data)
        {
            var entity = await entityComponent.ShowEntity(typeof(MyAircraft), "Aircraft",
                data);
            return entity.Logic as MyAircraft;
        }

        public static async UniTask<Aircraft> ShowAircraft(this GFEntityComponent entityComponent, AircraftData data)
        {
            var entity = await entityComponent.ShowEntity(typeof(Aircraft), "Aircraft", data);
            return entity.Logic as Aircraft;
        }

        public static async UniTask<Thruster> ShowThruster(this GFEntityComponent entityComponent, ThrusterData data)
        {
            var entity = await entityComponent.ShowEntity(typeof(Thruster), "Thruster", data);
            return entity.Logic as Thruster;
        }

        public static async UniTask<Weapon> ShowWeapon(this GFEntityComponent entityComponent, WeaponData data)
        {
            var entity = await entityComponent.ShowEntity(typeof(Weapon), "Weapon", data);
            return entity.Logic as Weapon;
        }

        public static async UniTask<Armor> ShowArmor(this GFEntityComponent entityComponent, ArmorData data)
        {
            var entity = await entityComponent.ShowEntity(typeof(Armor), "Armor", data);
            return entity.Logic as Armor;
        }

        public static async UniTask<Bullet> ShowBullet(this GFEntityComponent entityCompoennt, BulletData data)
        {
            var entity = await entityCompoennt.ShowEntity(typeof(Bullet), "Bullet", data);
            return entity.Logic as Bullet;
        }

        public static async UniTask<Asteroid> ShowAsteroid(this GFEntityComponent entityCompoennt, AsteroidData data)
        {
            var entity = await entityCompoennt.ShowEntity(typeof(Asteroid), "Asteroid", data);
            return entity.Logic as Asteroid;
        }

        public static async UniTask<Effect> ShowEffect(this GFEntityComponent entityComponent, EffectData data)
        {
            var entity = await entityComponent.ShowEntity(typeof(Effect), "Effect", data);
            return entity.Logic as Effect;
        }

        private static async UniTask<GFEntity> ShowEntity(this GFEntityComponent entityComponent, Type logicType,
            string entityGroup,
            EntityData data)
        {
            if (data == null)
            {
                Log.Warning("Data is invalid.");
                return null;
            }

            var tbEntity = GameEntry.LubanDataTable.Tables.TbEntity;
            var drEntity = tbEntity.GetOrDefault(data.TypeId);

            if (drEntity == null)
            {
                Log.Warning($"Can not load entity id '{data.TypeId}' from data table.");
                return null;
            }

            return await entityComponent.ShowEntity(data.Id, logicType, drEntity.AssetName,
                entityGroup, data) as GFEntity;
        }

        public static int GenerateSerialId(this GFEntityComponent entityComponent)
        {
            return --s_SerialId;
        }
    }
}