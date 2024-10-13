using GFPro;
using UnityEngine;

namespace Game
{
    /// <summary>
    ///     游戏入口。
    /// </summary>
    public partial class GameEntry : MonoBehaviour
    {
        public static BuiltinDataComponent BuiltinData { get; private set; }

        public static HPBarComponent HPBar { get; private set; }


        public static LubanDataTableComponent LubanDataTable { get; private set; }

        private static void InitCustomComponents()
        {
            BuiltinData = GFPro.GameEntry.GetComponent<BuiltinDataComponent>();
            HPBar = GFPro.GameEntry.GetComponent<HPBarComponent>();
            LubanDataTable = GFPro.GameEntry.GetComponent<LubanDataTableComponent>();
        }
    }
}