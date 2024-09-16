using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.ObjectPool;
using UnityGameFramework.Runtime;

namespace Game
{
    public class HPBarComponent : GameFrameworkComponent
    {
        [SerializeField] private HPBarItem m_HPBarItemTemplate;

        [SerializeField] private Transform m_HPBarInstanceRoot;

        [SerializeField] private int m_InstancePoolCapacity = 16;

        private List<HPBarItem> m_ActiveHPBarItems;
        private Canvas          m_CachedCanvas;

        private IObjectPool<HPBarItemObject> m_HPBarItemObjectPool;

        private void Start()
        {
            if (m_HPBarInstanceRoot == null)
            {
                Log.Error("You must set HP bar instance root first.");
                return;
            }

            m_CachedCanvas = m_HPBarInstanceRoot.GetComponent<Canvas>();
            m_HPBarItemObjectPool =
                GameEntry.ObjectPool.CreateSingleSpawnObjectPool<HPBarItemObject>("HPBarItem", m_InstancePoolCapacity);
            m_ActiveHPBarItems = new List<HPBarItem>();
        }

        private void Update()
        {
            for (var i = m_ActiveHPBarItems.Count - 1; i >= 0; i--)
            {
                var hpBarItem = m_ActiveHPBarItems[i];
                if (hpBarItem.Refresh()) continue;

                HideHPBar(hpBarItem);
            }
        }

        private void OnDestroy()
        {
        }

        public void ShowHPBar(Entity entity, float fromHPRatio, float toHPRatio)
        {
            if (entity == null)
            {
                Log.Warning("Entity is invalid.");
                return;
            }

            var hpBarItem = GetActiveHPBarItem(entity);
            if (hpBarItem == null)
            {
                hpBarItem = CreateHPBarItem(entity);
                m_ActiveHPBarItems.Add(hpBarItem);
            }

            hpBarItem.Init(entity, m_CachedCanvas, fromHPRatio, toHPRatio);
        }

        private void HideHPBar(HPBarItem hpBarItem)
        {
            hpBarItem.Reset();
            m_ActiveHPBarItems.Remove(hpBarItem);
            m_HPBarItemObjectPool.Unspawn(hpBarItem);
        }

        private HPBarItem GetActiveHPBarItem(Entity entity)
        {
            if (entity == null) return null;

            for (var i = 0; i < m_ActiveHPBarItems.Count; i++)
                if (m_ActiveHPBarItems[i].Owner == entity)
                    return m_ActiveHPBarItems[i];

            return null;
        }

        private HPBarItem CreateHPBarItem(Entity entity)
        {
            HPBarItem hpBarItem = null;
            var hpBarItemObject = m_HPBarItemObjectPool.Spawn();
            if (hpBarItemObject != null)
            {
                hpBarItem = (HPBarItem)hpBarItemObject.Target;
            }
            else
            {
                hpBarItem = Instantiate(m_HPBarItemTemplate);
                var transform = hpBarItem.GetComponent<Transform>();
                transform.SetParent(m_HPBarInstanceRoot);
                transform.localScale = Vector3.one;
                m_HPBarItemObjectPool.Register(HPBarItemObject.Create(hpBarItem), true);
            }

            return hpBarItem;
        }
    }
}