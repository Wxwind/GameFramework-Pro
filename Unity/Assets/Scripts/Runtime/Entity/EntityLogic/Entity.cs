﻿using GFPro;
using GFPro.Entity;
using UnityEngine;

namespace Game
{
    public abstract class Entity : EntityLogic
    {
        [SerializeField] private EntityData m_EntityData;

        public int Id => Entity.Id;

        public Animation CachedAnimation { get; private set; }


        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            CachedAnimation = GetComponent<Animation>();
        }


        protected override void OnShow(object userData)
        {
            base.OnShow(userData);

            m_EntityData = userData as EntityData;
            if (m_EntityData == null)
            {
                Log.Error("Entity data is invalid.");
                return;
            }

            Name = Utility.Text.Format("[Entity {0}]", Id);
            CachedTransform.localPosition = m_EntityData.Position;
            CachedTransform.localRotation = m_EntityData.Rotation;
            CachedTransform.localScale = Vector3.one;
        }
    }
}