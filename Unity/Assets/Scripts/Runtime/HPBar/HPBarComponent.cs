using System.Collections.Generic;
using GFPro;
using GFPro.GFObjectPool;
using UnityEngine;

namespace Game
{
    public class HPBarComponent : Entity, IAwake
    {
        [SerializeField] private HPBarItem m_HPBarItemTemplate;

        [SerializeField] private Transform m_HPBarInstanceRoot;

        [SerializeField] private int m_InstancePoolCapacity = 16;

        private List<HPBarItem> m_ActiveHPBarItems;
        private Canvas          m_CachedCanvas;

        private IObjectPool<HPBarItemObject> m_HPBarItemObjectPool;

        public void Awake()
        {
            if (m_HPBarInstanceRoot == null)
            {
                Log.Error("You must set HP bar instance root first.");
                return;
            }

            m_CachedCanvas = m_HPBarInstanceRoot.GetComponent<Canvas>();
            m_HPBarItemObjectPool =
                ObjectPool.Instance.CreateSingleSpawnObjectPool<HPBarItemObject>("HPBarItem", m_InstancePoolCapacity);
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

        public void ShowHPBar(GameEntity gameEntity, float fromHPRatio, float toHPRatio)
        {
            if (gameEntity == null)
            {
                Log.Warning("Entity is invalid.");
                return;
            }

            var hpBarItem = GetActiveHPBarItem(gameEntity);
            if (hpBarItem == null)
            {
                hpBarItem = CreateHPBarItem(gameEntity);
                m_ActiveHPBarItems.Add(hpBarItem);
            }

            hpBarItem.Init(gameEntity, m_CachedCanvas, fromHPRatio, toHPRatio);
        }

        private void HideHPBar(HPBarItem hpBarItem)
        {
            hpBarItem.Reset();
            m_ActiveHPBarItems.Remove(hpBarItem);
            m_HPBarItemObjectPool.Unspawn(hpBarItem);
        }

        private HPBarItem GetActiveHPBarItem(GameEntity gameEntity)
        {
            if (gameEntity == null) return null;

            for (var i = 0; i < m_ActiveHPBarItems.Count; i++)
                if (m_ActiveHPBarItems[i].Owner == gameEntity)
                    return m_ActiveHPBarItems[i];

            return null;
        }

        private HPBarItem CreateHPBarItem(GameEntity gameEntity)
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