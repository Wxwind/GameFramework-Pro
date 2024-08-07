using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace Game
{
    public class HPBarItem : MonoBehaviour
    {
        private const float AnimationSeconds = 0.3f;
        private const float KeepSeconds      = 0.4f;
        private const float FadeOutSeconds   = 0.3f;

        [SerializeField] private Slider m_HPBar;

        private CanvasGroup   m_CachedCanvasGroup;
        private RectTransform m_CachedTransform;
        private int           m_OwnerId;

        private Canvas m_ParentCanvas;

        public Entity Owner { get; private set; }

        private void Awake()
        {
            m_CachedTransform = GetComponent<RectTransform>();
            if (m_CachedTransform == null)
            {
                Log.Error("RectTransform is invalid.");
                return;
            }

            m_CachedCanvasGroup = GetComponent<CanvasGroup>();
            if (m_CachedCanvasGroup == null) Log.Error("CanvasGroup is invalid.");
        }

        public void Reset()
        {
            StopAllCoroutines();
            m_CachedCanvasGroup.alpha = 1f;
            m_HPBar.value = 1f;
            Owner = null;
            gameObject.SetActive(false);
        }

        public void Init(Entity owner, Canvas parentCanvas, float fromHPRatio, float toHPRatio)
        {
            if (owner == null)
            {
                Log.Error("Owner is invalid.");
                return;
            }

            m_ParentCanvas = parentCanvas;

            gameObject.SetActive(true);
            StopAllCoroutines();

            m_CachedCanvasGroup.alpha = 1f;
            if (Owner != owner || m_OwnerId != owner.Id)
            {
                m_HPBar.value = fromHPRatio;
                Owner = owner;
                m_OwnerId = owner.Id;
            }

            Refresh();

            StartCoroutine(HPBarCo(toHPRatio, AnimationSeconds, KeepSeconds, FadeOutSeconds));
        }

        public bool Refresh()
        {
            if (m_CachedCanvasGroup.alpha <= 0f) return false;

            if (Owner != null && Owner.Available && Owner.Id == m_OwnerId)
            {
                var worldPosition = Owner.CachedTransform.position + Vector3.forward;
                var screenPosition = GameEntry.Scene.MainCamera.WorldToScreenPoint(worldPosition);

                Vector2 position;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)m_ParentCanvas.transform,
                        screenPosition,
                        m_ParentCanvas.worldCamera, out position))
                    m_CachedTransform.localPosition = position;
            }

            return true;
        }

        private IEnumerator HPBarCo(float value, float animationDuration, float keepDuration, float fadeOutDuration)
        {
            yield return m_HPBar.SmoothValue(value, animationDuration);
            yield return new WaitForSeconds(keepDuration);
            yield return m_CachedCanvasGroup.FadeToAlpha(0f, fadeOutDuration);
        }
    }
}