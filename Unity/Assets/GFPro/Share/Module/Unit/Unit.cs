using System.Diagnostics;
using MongoDB.Bson.Serialization.Attributes;
using Unity.Mathematics;

namespace GFPro
{
    [ChildOf(typeof(UnitComponent))]
    [DebuggerDisplay("ViewName,nq")]
    public class Unit : Entity, IAwake<int>
    {
        public int ConfigId { get; set; } //配置表id

        [BsonIgnore] public UnitConfig Config => UnitConfigCategory.Instance.Get(ConfigId);

        public UnitType Type => (UnitType)UnitConfigCategory.Instance.Get(ConfigId).Type;

        [BsonElement] private float3 position; //坐标

        [BsonIgnore]
        public float3 Position
        {
            get => position;
            set
            {
                var oldPos = position;
                position = value;
                EventSystem.Instance.Publish(this.DomainScene(), new EventType.ChangePosition() { Unit = this, OldPos = oldPos });
            }
        }

        [BsonIgnore]
        public float3 Forward
        {
            get => math.mul(Rotation, math.forward());
            set => Rotation = quaternion.LookRotation(value, math.up());
        }

        [BsonElement] private quaternion rotation;

        [BsonIgnore]
        public quaternion Rotation
        {
            get => rotation;
            set
            {
                rotation = value;
                EventSystem.Instance.Publish(this.DomainScene(), new EventType.ChangeRotation() { Unit = this });
            }
        }

        protected override string ViewName => $"{GetType().Name} ({Id})";
    }
}