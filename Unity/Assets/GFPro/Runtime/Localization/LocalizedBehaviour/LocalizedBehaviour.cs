using UnityEngine;

namespace GFPro
{
    public abstract class LocalizedBehaviour<T> : MonoBehaviour where T : UnityEngine.Object
    {
        public    string TranslationKey;
        protected T      m_Target;

        protected virtual void OnEnable()
        {
            LocalizationComponent.Instance.OnLanguageChanged += TranslationInternal;
            TranslationInternal();
        }

        protected virtual void OnDisable()
        {
            LocalizationComponent.Instance.OnLanguageChanged -= TranslationInternal;
        }

        protected abstract void TranslationInternal();
    }
}