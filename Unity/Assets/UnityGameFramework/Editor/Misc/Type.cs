﻿using System.Collections.Generic;
using System.Reflection;

namespace GFPro.Editor
{
    /// <summary>
    /// 类型相关的实用函数。
    /// </summary>
    internal static class Type
    {
        private static readonly string[] RuntimeAssemblyNames =
        {
            "UnityGameFramework.Runtime",
            "Game.Runtime"
        };

        private static readonly string[] RuntimeOrEditorAssemblyNames =
        {
            "UnityGameFramework.Runtime",
            "Game.Runtime",
            "UnityGameFramework.Editor",
            "Game.Editor"
        };

        /// <summary>
        /// 在运行时程序集中获取指定基类的所有子类的名称。
        /// </summary>
        /// <param name="typeBase">基类类型。</param>
        /// <returns>指定基类的所有子类的名称。</returns>
        internal static string[] GetRuntimeTypeNames(System.Type typeBase)
        {
            return GetTypeNames(typeBase, RuntimeAssemblyNames);
        }

        /// <summary>
        /// 在运行时或编辑器程序集中获取指定基类的所有子类的名称。
        /// </summary>
        /// <param name="typeBase">基类类型。</param>
        /// <returns>指定基类的所有子类的名称。</returns>
        internal static string[] GetRuntimeOrEditorTypeNames(System.Type typeBase)
        {
            return GetTypeNames(typeBase, RuntimeOrEditorAssemblyNames);
        }

        private static string[] GetTypeNames(System.Type typeBase, string[] assemblyNames)
        {
            var typeNames = new List<string>();
            foreach (var assemblyName in assemblyNames)
            {
                Assembly assembly = null;
                try
                {
                    assembly = Assembly.Load(assemblyName);
                }
                catch
                {
                    continue;
                }

                if (assembly == null) continue;

                var types = assembly.GetTypes();
                foreach (var type in types)
                    if (type.IsClass && !type.IsAbstract && typeBase.IsAssignableFrom(type))
                        typeNames.Add(type.FullName);
            }

            typeNames.Sort();
            return typeNames.ToArray();
        }
    }
}