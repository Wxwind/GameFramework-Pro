using GFPro;
using GFPro.Entity;
using GFPro.Fsm;
using GFPro.ObjectPool;
using GFPro.Procedure;
using UnityEngine;

namespace Game
{
    /// <summary>
    ///     游戏入口。
    /// </summary>
    public partial class GameEntry : MonoBehaviour
    {
        /// <summary>
        ///     获取游戏基础组件。
        /// </summary>
        public static BaseComponent Base { get; private set; }

        /// <summary>
        ///     获取配置组件。
        /// </summary>
        public static ConfigComponent Config { get; private set; }


        /// <summary>
        ///     获取调试组件。
        /// </summary>
        public static DebuggerComponent Debugger { get; private set; }

        /// <summary>
        ///     获取实体组件。
        /// </summary>
        public static EntityComponent Entity { get; private set; }

        /// <summary>
        ///     获取事件组件。
        /// </summary>
        public static EventComponent Event { get; private set; }

        /// <summary>
        ///     获取文件系统组件。
        /// </summary>
        public static FileSystemComponent FileSystem { get; private set; }

        /// <summary>
        ///     获取有限状态机组件。
        /// </summary>
        public static FsmComponent Fsm { get; private set; }

        /// <summary>
        ///     获取本地化组件。
        /// </summary>
        public static LocalizationComponent Localization { get; private set; }


        /// <summary>
        ///     获取对象池组件。
        /// </summary>
        public static ObjectPoolComponent ObjectPool { get; private set; }

        /// <summary>
        ///     获取流程组件。
        /// </summary>
        public static ProcedureComponent Procedure { get; private set; }

        /// <summary>
        ///     获取资源组件。
        /// </summary>
        public static ResourceComponent Resource { get; private set; }

        /// <summary>
        ///     获取场景组件。
        /// </summary>
        public static SceneComponent Scene { get; private set; }

        /// <summary>
        ///     获取配置组件。
        /// </summary>
        public static SettingComponent Setting { get; private set; }

        /// <summary>
        ///     获取声音组件。
        /// </summary>
        public static SoundComponent Sound { get; private set; }

        /// <summary>
        ///     获取界面组件。
        /// </summary>
        public static UIComponent UI { get; private set; }


        private static void InitBuiltinComponents()
        {
            Base = GFPro.GameEntry.GetComponent<BaseComponent>();
            Config = GFPro.GameEntry.GetComponent<ConfigComponent>();
            Debugger = GFPro.GameEntry.GetComponent<DebuggerComponent>();
            Entity = GFPro.GameEntry.GetComponent<EntityComponent>();
            Event = GFPro.GameEntry.GetComponent<EventComponent>();
            FileSystem = GFPro.GameEntry.GetComponent<FileSystemComponent>();
            Fsm = GFPro.GameEntry.GetComponent<FsmComponent>();
            Localization = GFPro.GameEntry.GetComponent<LocalizationComponent>();
            ObjectPool = GFPro.GameEntry.GetComponent<ObjectPoolComponent>();
            Procedure = GFPro.GameEntry.GetComponent<ProcedureComponent>();
            Resource = GFPro.GameEntry.GetComponent<ResourceComponent>();
            Scene = GFPro.GameEntry.GetComponent<SceneComponent>();
            Setting = GFPro.GameEntry.GetComponent<SettingComponent>();
            Sound = GFPro.GameEntry.GetComponent<SoundComponent>();
            UI = GFPro.GameEntry.GetComponent<UIComponent>();
        }
    }
}