using UnityEngine;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 界面字符型主键。
    /// </summary>
    public sealed class UIStringKey : MonoBehaviour
    {
        [SerializeField] private string m_Key;

        /// <summary>
        /// 获取或设置主键。
        /// </summary>
        public string Key
        {
            get => m_Key ?? string.Empty;
            set => m_Key = value;
        }
    }
}