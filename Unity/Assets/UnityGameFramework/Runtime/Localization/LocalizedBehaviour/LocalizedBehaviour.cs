using UnityEngine;

namespace GFPro
{
    public abstract class LocalizedBehaviour<T> : MonoBehaviour where T : Object
    {
        public    string TranslationKey;
        protected T      m_Target;

        protected virtual void OnEnable()
        {
            GameEntry.GetComponent<LocalizationComponent>().OnLanguageChanged += TranslationInternal;
            TranslationInternal();
        }

        protected virtual void OnDisable()
        {
            GameEntry.GetComponent<LocalizationComponent>().OnLanguageChanged -= TranslationInternal;
        }

        protected abstract void TranslationInternal();
    }
}