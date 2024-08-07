﻿using UnityEngine;
using UnityGameFramework.Runtime;

namespace Game
{
    /// <summary>
    ///     游戏入口。
    /// </summary>
    public partial class GameEntry : MonoBehaviour
    {
        public static BuiltinDataComponent BuiltinData { get; private set; }

        public static HPBarComponent HPBar { get; private set; }


        public static LubanConfigComponent LubanConfig { get; private set; }

        private static void InitCustomComponents()
        {
            BuiltinData = UnityGameFramework.Runtime.GameEntry.GetComponent<BuiltinDataComponent>();
            HPBar = UnityGameFramework.Runtime.GameEntry.GetComponent<HPBarComponent>();
            LubanConfig = UnityGameFramework.Runtime.GameEntry.GetComponent<LubanConfigComponent>();
        }
    }
}