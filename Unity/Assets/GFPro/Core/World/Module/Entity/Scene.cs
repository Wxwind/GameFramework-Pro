using System.Diagnostics;

namespace GFPro
{
    [DebuggerDisplay("ViewName,nq")]
    public sealed class Scene : Entity
    {
        public int Zone { get; }

        public SceneType SceneType { get; }

        public string Name { get; }

        public Scene(long instanceId, int zone, SceneType sceneType, string name, Entity parent)
        {
            Id = instanceId;
            InstanceId = instanceId;
            Zone = zone;
            SceneType = sceneType;
            Name = name;
            IsCreated = true;
            IsNew = true;
            Parent = parent;
            Domain = this;
            IsRegister = true;
            Log.Info($"scene create: {SceneType} {Name} {Id} {InstanceId} {Zone}");
        }

        public Scene(long id, long instanceId, int zone, SceneType sceneType, string name, Entity parent)
        {
            Id = id;
            InstanceId = instanceId;
            Zone = zone;
            SceneType = sceneType;
            Name = name;
            IsCreated = true;
            IsNew = true;
            Parent = parent;
            Domain = this;
            IsRegister = true;
            Log.Info($"scene create: {SceneType} {Name} {Id} {InstanceId} {Zone}");
        }

        public override void Dispose()
        {
            base.Dispose();

            Log.Info($"scene dispose: {SceneType} {Name} {Id} {InstanceId} {Zone}");
        }

        public new Entity Domain
        {
            get => domain;
            private set => domain = value;
        }

        public new Entity Parent
        {
            get => parent;
            private set
            {
                if (value == null)
                {
                    //this.parent = this;
                    return;
                }

                parent = value;
                parent.Children.Add(Id, this);
            }
        }

        protected override string ViewName => $"{GetType().Name} ({SceneType})";
    }
}