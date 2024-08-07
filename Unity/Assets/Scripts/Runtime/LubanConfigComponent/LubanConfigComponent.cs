﻿using System;
using Cysharp.Threading.Tasks;
using Game.Config;
using GameFramework;
using Luban;
using SimpleJSON;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public enum TablesLoadType : byte
    {
        Undefined = 0,
        Bytes,
        Json
    }

    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework Pro/Luban Config")]
    public class LubanConfigComponent : GameFrameworkComponent
    {
        public TablesLoadType LoadType { get; private set; } = TablesLoadType.Undefined;

        public Tables Tables { get; private set; }

        public async UniTask LoadAsync()
        {
            var tables = new Tables();

            var tablesType = typeof(Tables);
            var loadMethodInfo = tablesType.GetMethod("LoadAsync");
            var loaderReturnType = loadMethodInfo.GetParameters()[0].ParameterType.GetGenericArguments()[1];
            // 根据cfg.Tables的构造函数的Loader的返回值类型决定使用json还是ByteBuf Loader
            if (loaderReturnType == typeof(UniTask<ByteBuf>))
            {
                LoadType = TablesLoadType.Bytes;

                async UniTask<ByteBuf> LoadByteBuf(string file)
                {
                    var textAsset = await GameEntry.GetComponent<ResourceComponent>().LoadAssetAsync<TextAsset>(file);
                    return new ByteBuf(textAsset.bytes);
                }

                Func<string, UniTask<ByteBuf>> func = LoadByteBuf;
                await (UniTask)loadMethodInfo.Invoke(tables, new object[] { func });
            }
            else if (loaderReturnType == typeof(UniTask<JSONNode>))
            {
                LoadType = TablesLoadType.Json;

                async UniTask<JSONNode> LoadJson(string file)
                {
                    var textAsset = await Game.GameEntry.Resource.LoadAssetAsync<TextAsset>(file);
                    return JSON.Parse(textAsset.text);
                }

                Func<string, UniTask<JSONNode>> func = LoadJson;
                await (UniTask)loadMethodInfo.Invoke(tables, new object[] { func });
            }
            else
            {
                throw new GameFrameworkException($"{loaderReturnType.FullName} is not supported.");
            }


            Tables = tables;
        }
    }
}