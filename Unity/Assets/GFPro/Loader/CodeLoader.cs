using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace GFPro
{
    public class CodeLoader : Singleton<CodeLoader>, ISingletonAwake
    {
        private Assembly modelAssembly;
        private Assembly modelViewAssembly;

        private Dictionary<string, TextAsset> dlls;
        private Dictionary<string, TextAsset> aotDlls;
        private bool                          enableDll;

        public void Awake()
        {
            enableDll = Resources.Load<GlobalConfig>("GlobalConfig").EnableDll;
        }

        public async ETTask DownloadAsync()
        {
            if (!Define.IsEditor)
            {
                dlls = await ResourceComponent.Instance.LoadAllAssetsAsync<TextAsset>($"Assets/Bundles/Code/Unity.Model.dll.bytes");
                aotDlls = await ResourceComponent.Instance.LoadAllAssetsAsync<TextAsset>($"Assets/Bundles/AotDlls/mscorlib.dll.bytes");
            }
        }

        public void Start()
        {
            if (!Define.IsEditor)
            {
                var modelAssBytes = dlls["Unity.Model.dll"].bytes;
                var modelPdbBytes = dlls["Unity.Model.pdb"].bytes;
                var modelViewAssBytes = dlls["Unity.ModelView.dll"].bytes;
                var modelViewPdbBytes = dlls["Unity.ModelView.pdb"].bytes;
                // 如果需要测试，可替换成下面注释的代码直接加载Assets/Bundles/Code/Unity.Model.dll.bytes，但真正打包时必须使用上面的代码
                //modelAssBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.Model.dll.bytes"));
                //modelPdbBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.Model.pdb.bytes"));
                //modelViewAssBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.ModelView.dll.bytes"));
                //modelViewPdbBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.ModelView.pdb.bytes"));

                // if (Define.EnableIL2CPP)
                // {
                //     foreach (var kv in aotDlls)
                //     {
                //         var textAsset = kv.Value;
                //         RuntimeApi.LoadMetadataForAOTAssembly(textAsset.bytes, HomologousImageMode.SuperSet);
                //     }
                // }

                modelAssembly = Assembly.Load(modelAssBytes, modelPdbBytes);
                modelViewAssembly = Assembly.Load(modelViewAssBytes, modelViewPdbBytes);
            }
            else
            {
                if (enableDll)
                {
                    var modelAssBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.Model.dll.bytes"));
                    var modelPdbBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.Model.pdb.bytes"));
                    var modelViewAssBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.ModelView.dll.bytes"));
                    var modelViewPdbBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.ModelView.pdb.bytes"));
                    modelAssembly = Assembly.Load(modelAssBytes, modelPdbBytes);
                    modelViewAssembly = Assembly.Load(modelViewAssBytes, modelViewPdbBytes);
                }
                else
                {
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (var ass in assemblies)
                    {
                        var name = ass.GetName().Name;
                        if (name == "Unity.Model")
                        {
                            modelAssembly = ass;
                        }
                        else if (name == "Unity.ModelView")
                        {
                            modelViewAssembly = ass;
                        }

                        if (modelAssembly != null && modelViewAssembly != null)
                        {
                            break;
                        }
                    }
                }
            }

            var (hotfixAssembly, hotfixViewAssembly) = LoadHotfix();

            World.Instance.AddSingleton<CodeTypes, Assembly[]>(new[]
            {
                typeof(World).Assembly, typeof(Init).Assembly, modelAssembly, modelViewAssembly, hotfixAssembly,
                hotfixViewAssembly
            });

            IStaticMethod start = new StaticMethod(modelAssembly, "GFPro.Entry", "Start");
            start.Run();
        }

        private (Assembly, Assembly) LoadHotfix()
        {
            byte[] hotfixAssBytes;
            byte[] hotfixPdbBytes;
            byte[] hotfixViewAssBytes;
            byte[] hotfixViewPdbBytes;
            Assembly hotfixAssembly = null;
            Assembly hotfixViewAssembly = null;
            if (!Define.IsEditor)
            {
                hotfixAssBytes = dlls["Unity.Hotfix.dll"].bytes;
                hotfixPdbBytes = dlls["Unity.Hotfix.pdb"].bytes;
                hotfixViewAssBytes = dlls["Unity.HotfixView.dll"].bytes;
                hotfixViewPdbBytes = dlls["Unity.HotfixView.pdb"].bytes;
                // 如果需要测试，可替换成下面注释的代码直接加载Assets/Bundles/Code/Hotfix.dll.bytes，但真正打包时必须使用上面的代码
                //hotfixAssBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.Hotfix.dll.bytes"));
                //hotfixPdbBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.Hotfix.pdb.bytes"));
                //hotfixViewAssBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.HotfixView.dll.bytes"));
                //hotfixViewPdbBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.HotfixView.pdb.bytes"));
                hotfixAssembly = Assembly.Load(hotfixAssBytes, hotfixPdbBytes);
                hotfixViewAssembly = Assembly.Load(hotfixViewAssBytes, hotfixViewPdbBytes);
            }
            else
            {
                if (enableDll)
                {
                    hotfixAssBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.Hotfix.dll.bytes"));
                    hotfixPdbBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.Hotfix.pdb.bytes"));
                    hotfixViewAssBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.HotfixView.dll.bytes"));
                    hotfixViewPdbBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.HotfixView.pdb.bytes"));
                    hotfixAssembly = Assembly.Load(hotfixAssBytes, hotfixPdbBytes);
                    hotfixViewAssembly = Assembly.Load(hotfixViewAssBytes, hotfixViewPdbBytes);
                }
                else
                {
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (var ass in assemblies)
                    {
                        var name = ass.GetName().Name;
                        if (name == "Unity.Hotfix")
                        {
                            hotfixAssembly = ass;
                        }
                        else if (name == "Unity.HotfixView")
                        {
                            hotfixViewAssembly = ass;
                        }

                        if (hotfixAssembly != null && hotfixViewAssembly != null)
                        {
                            break;
                        }
                    }
                }
            }

            return (hotfixAssembly, hotfixViewAssembly);
        }

        public void Reload()
        {
            var (hotfixAssembly, hotfixViewAssembly) = LoadHotfix();

            var codeTypes = World.Instance.AddSingleton<CodeTypes, Assembly[]>(new[]
            {
                typeof(World).Assembly, typeof(Init).Assembly, modelAssembly, modelViewAssembly, hotfixAssembly,
                hotfixViewAssembly
            });
            codeTypes.CreateCode();

            Log.Info($"reload dll finish!");
        }
    }
}