using System;
using UnityEngine;

namespace GFPro
{
    public sealed partial class GFEntityComponent
    {
        [Serializable]
        private sealed class EntityGroupConfig
        {
            [SerializeField] private string m_Name;

            [SerializeField] private float m_InstanceAutoReleaseInterval = 60f;

            [SerializeField] private int m_InstanceCapacity = 16;

            [SerializeField] private float m_InstanceExpireTime = 60f;

            [SerializeField] private int m_InstancePriority;

            public string Name => m_Name;

            public float InstanceAutoReleaseInterval => m_InstanceAutoReleaseInterval;

            public int InstanceCapacity => m_InstanceCapacity;

            public float InstanceExpireTime => m_InstanceExpireTime;

            public int InstancePriority => m_InstancePriority;
        }
    }
}