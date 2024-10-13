using GFPro;
using UnityEngine;

namespace Game
{
    /// <summary>
    ///     游戏入口。
    /// </summary>
    public partial class GameEntry : MonoBehaviour
    {
        public static HPBarComponent HPBar { get; private set; }


        public static LubanDataTableComponent LubanDataTable { get; private set; }

        private static void InitCustomComponents()
        {
            HPBar = GFPro.GameEntry.GetComponent<HPBarComponent>();
            LubanDataTable = GFPro.GameEntry.GetComponent<LubanDataTableComponent>();
        }
    }
}