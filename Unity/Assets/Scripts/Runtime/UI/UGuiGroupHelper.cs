﻿using GFPro.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    /// <summary>
    ///     uGUI 界面组辅助器。
    /// </summary>
    public class UGuiGroupHelper : MonoBehaviour, IUIGroupHelper
    {
        public const int    DepthFactor = 10000;
        private      Canvas m_CachedCanvas;

        private int m_Depth;

        private UGuiGroupHelper(GameObject gameObject)
        {
        }

        private void Awake()
        {
            m_CachedCanvas = gameObject.GetOrAddComponent<Canvas>();
            gameObject.GetOrAddComponent<GraphicRaycaster>();
        }

        private void Start()
        {
            m_CachedCanvas.overrideSorting = true;
            m_CachedCanvas.sortingOrder = DepthFactor * m_Depth;

            var transform = GetComponent<RectTransform>();
            transform.anchorMin = Vector2.zero;
            transform.anchorMax = Vector2.one;
            transform.anchoredPosition = Vector2.zero;
            transform.sizeDelta = Vector2.zero;
        }

        /// <summary>
        ///     设置界面组深度。
        /// </summary>
        /// <param name="depth">界面组深度。</param>
        public void SetDepth(int depth)
        {
            m_Depth = depth;
            m_CachedCanvas.overrideSorting = true;
            m_CachedCanvas.sortingOrder = DepthFactor * depth;
        }
    }
}