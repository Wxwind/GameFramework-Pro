using UnityEngine;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 界面整型主键。
    /// </summary>
    public sealed class UIIntKey : MonoBehaviour
    {
        [SerializeField] private int m_Key;

        /// <summary>
        /// 获取或设置主键。
        /// </summary>
        public int Key
        {
            get => m_Key;
            set => m_Key = value;
        }
    }
}