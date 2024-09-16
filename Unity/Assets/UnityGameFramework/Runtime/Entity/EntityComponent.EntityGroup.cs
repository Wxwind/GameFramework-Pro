using System;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public sealed partial class EntityComponent : GameFrameworkComponent
    {
        [Serializable]
        private sealed class EntityGroup
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