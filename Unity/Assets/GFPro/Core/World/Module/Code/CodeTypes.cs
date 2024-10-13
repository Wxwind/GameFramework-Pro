using System;
using System.Collections.Generic;
using System.Reflection;

namespace GFPro
{
    public class CodeTypes : Singleton<CodeTypes>, ISingletonAwake<Assembly[]>
    {
        private readonly Dictionary<string, Type>       allTypes = new();
        private readonly UnOrderMultiMapSet<Type, Type> types    = new();

        public void Awake(Assembly[] assemblies)
        {
            var addTypes = AssemblyHelper.GetAssemblyTypes(assemblies);
            foreach (var (fullName, type) in addTypes)
            {
                allTypes[fullName] = type;

                if (type.IsAbstract)
                {
                    continue;
                }

                // 记录所有的有BaseAttribute标记的的类型
                var objects = type.GetCustomAttributes(typeof(BaseAttribute), true);

                foreach (var o in objects)
                {
                    types.Add(o.GetType(), type);
                }
            }
        }

        public HashSet<Type> GetTypes(Type systemAttributeType)
        {
            if (!types.ContainsKey(systemAttributeType))
            {
                return new HashSet<Type>();
            }

            return types[systemAttributeType];
        }

        public Dictionary<string, Type> GetTypes()
        {
            return allTypes;
        }

        public Type GetType(string typeName)
        {
            return allTypes[typeName];
        }

        public void CreateCode()
        {
            var hashSet = GetTypes(typeof(CodeAttribute));
            foreach (var type in hashSet)
            {
                var obj = Activator.CreateInstance(type);
                ((ISingletonAwake)obj).Awake();
                World.Instance.AddSingleton((ISingleton)obj);
            }
        }
    }
}