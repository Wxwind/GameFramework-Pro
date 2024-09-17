using System;

namespace UnityGameFramework.Entity
{
    internal sealed class ShowEntityInfo : IReference
    {
        private Type   m_EntityLogicType;
        private object m_UserData;

        public Type EntityLogicType => m_EntityLogicType;

        public object UserData => m_UserData;

        public static ShowEntityInfo Create(Type entityLogicType, object userData)
        {
            var showEntityInfo = ReferencePool.Acquire<ShowEntityInfo>();
            showEntityInfo.m_EntityLogicType = entityLogicType;
            showEntityInfo.m_UserData = userData;
            return showEntityInfo;
        }

        public void Clear()
        {
            m_EntityLogicType = null;
            m_UserData = null;
        }
    }
}