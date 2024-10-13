using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GFPro
{
    // 管理根部的Scene
    public class Root : Singleton<Root>, ISingletonAwake
    {
        // 管理所有的Entity
        private readonly Dictionary<long, Entity> allEntities = new();

        public Scene Scene { get; private set; }

        public void Awake()
        {
            Scene = EntitySceneFactory.CreateScene(0, SceneType.Process, "Process");
        }

        public override void Dispose()
        {
            Scene.Dispose();
        }

        public void Add(Entity entity)
        {
            allEntities.Add(entity.InstanceId, entity);
        }

        public void Remove(long instanceId)
        {
            allEntities.Remove(instanceId);
        }

        public Entity Get(long instanceId)
        {
            Entity component = null;
            allEntities.TryGetValue(instanceId, out component);
            return component;
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            var noParent = new HashSet<Type>();
            var typeCount = new Dictionary<Type, int>();

            var noDomain = new HashSet<Type>();

            foreach (var kv in allEntities)
            {
                var type = kv.Value.GetType();
                if (kv.Value.Parent == null)
                {
                    noParent.Add(type);
                }

                if (kv.Value.Domain == null)
                {
                    noDomain.Add(type);
                }

                if (typeCount.ContainsKey(type))
                {
                    typeCount[type]++;
                }
                else
                {
                    typeCount[type] = 1;
                }
            }

            sb.AppendLine("not set parent type: ");
            foreach (var type in noParent)
            {
                sb.AppendLine($"\t{type.Name}");
            }

            sb.AppendLine("not set domain type: ");
            foreach (var type in noDomain)
            {
                sb.AppendLine($"\t{type.Name}");
            }

            var orderByDescending = typeCount.OrderByDescending(s => s.Value);

            sb.AppendLine("Entity Count: ");
            foreach (var kv in orderByDescending)
            {
                if (kv.Value == 1)
                {
                    continue;
                }

                sb.AppendLine($"\t{kv.Key.Name}: {kv.Value}");
            }

            return sb.ToString();
        }
    }
}