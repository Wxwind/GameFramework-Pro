using System.IO;
using cfg;
using SimpleJSON;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework Pro/Luban Config")]
    public class LubanConfigComponent : GameFrameworkComponent
    {
        private Tables _tables;

        public Tables Tables
        {
            get
            {
                if (_tables == null)
                {
                    _tables = Load();
                }

                return _tables;
            }
        }

        private Tables Load()
        {
            var tables = new Tables(LoadByJson);
            return tables;
        }

        private JSONNode LoadByJson(string fileName)
        {
            // TODO: 使用yooasset加载
            var text = File.ReadAllText(Application.dataPath + "/AssetRes/LubanConfig/" + fileName + ".json");
            return JSON.Parse(text);
        }
    }
}