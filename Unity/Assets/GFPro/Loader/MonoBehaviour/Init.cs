using System;
using CommandLine;
using UnityEngine;

namespace GFPro
{
    public class Init : MonoBehaviour
    {
        private void Start()
        {
            DontDestroyOnLoad(gameObject);

            AppDomain.CurrentDomain.UnhandledException += (sender, e) => { Log.Error(e.ExceptionObject.ToString()); };

            World.Instance.AddSingleton<MainThreadSynchronizationContext>();

            // 命令行参数
            var args = "".Split(" ");
            Parser.Default.ParseArguments<Options>(args)
                .WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
                .WithParsed(World.Instance.AddSingleton);

            World.Instance.AddSingleton<TimeInfo>();
            World.Instance.AddSingleton<Logger>().ILog = new UnityLogger();
            World.Instance.AddSingleton<ObjectPool>();
            World.Instance.AddSingleton<IdGenerater>();
            World.Instance.AddSingleton<EventSystem>();
            World.Instance.AddSingleton<TimerComponent>();
            World.Instance.AddSingleton<CoroutineLockComponent>();

            World.Instance.AddSingleton<ResourceComponent>();

            World.Instance.AddSingleton<CodeLoader>().Start();


            ETTask.ExceptionHandler += Log.Error;
        }

        private void Update()
        {
            World.Instance.Update();
        }

        private void LateUpdate()
        {
            World.Instance.LateUpdate();
            World.Instance.FrameFinishUpdate();
        }

        private void OnApplicationQuit()
        {
            World.Instance.Close();
        }
    }
}