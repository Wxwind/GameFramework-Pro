using System;
using System.Collections.Generic;
using System.Reflection;

namespace GFPro
{
    public static class AssemblyHelper
    {
        public static Dictionary<string, Type> GetAssemblyTypes(params Assembly[] args)
        {
            var types = new Dictionary<string, Type>();

            foreach (var ass in args)
            {
                foreach (var type in ass.GetTypes())
                {
                    types[type.FullName] = type;
                }
            }

            return types;
        }
    }
}