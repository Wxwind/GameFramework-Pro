using UnityEngine;

namespace GFPro.Entity
{
    internal sealed class AttachEntityInfo : IReference
    {
        private Transform m_ParentTransform;
        private object    m_UserData;

        public AttachEntityInfo()
        {
            m_ParentTransform = null;
            m_UserData = null;
        }

        public Transform ParentTransform => m_ParentTransform;

        public object UserData => m_UserData;

        public static AttachEntityInfo Create(Transform parentTransform, object userData)
        {
            var attachEntityInfo = ReferencePool.Acquire<AttachEntityInfo>();
            attachEntityInfo.m_ParentTransform = parentTransform;
            attachEntityInfo.m_UserData = userData;
            return attachEntityInfo;
        }

        public void Clear()
        {
            m_ParentTransform = null;
            m_UserData = null;
        }
    }
}