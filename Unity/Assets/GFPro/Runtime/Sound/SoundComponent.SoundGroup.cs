using System;
using UnityEngine;

namespace GFPro
{
    public sealed partial class SoundComponent
    {
        [Serializable]
        private sealed class SoundGroup
        {
            [SerializeField] private string m_Name;

            [SerializeField] private bool m_AvoidBeingReplacedBySamePriority;

            [SerializeField] private bool m_Mute;

            [SerializeField] [Range(0f, 1f)] private float m_Volume = 1f;

            [SerializeField] private int m_AgentHelperCount = 1;

            public string Name => m_Name;

            public bool AvoidBeingReplacedBySamePriority => m_AvoidBeingReplacedBySamePriority;

            public bool Mute => m_Mute;

            public float Volume => m_Volume;

            public int AgentHelperCount => m_AgentHelperCount;
        }
    }
}