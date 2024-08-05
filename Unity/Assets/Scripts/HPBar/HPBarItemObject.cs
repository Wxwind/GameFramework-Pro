﻿using GameFramework;
using GameFramework.ObjectPool;
using UnityEngine;

namespace GameMain
{
    public class HPBarItemObject : ObjectBase
    {
        public static HPBarItemObject Create(object target)
        {
            var hpBarItemObject = ReferencePool.Acquire<HPBarItemObject>();
            hpBarItemObject.Initialize(target);
            return hpBarItemObject;
        }

        protected override void Release(bool isShutdown)
        {
            var hpBarItem = (HPBarItem)Target;
            if (hpBarItem == null) return;

            Object.Destroy(hpBarItem.gameObject);
        }
    }
}