using System;
using UnityEngine;

namespace GFPro
{
    public sealed partial class UIComponent
    {
        [Serializable]
        private sealed class UIGroup
        {
            [SerializeField] private string m_Name;

            [SerializeField] private int m_Depth;

            public string Name => m_Name;

            public int Depth => m_Depth;
        }
    }
}