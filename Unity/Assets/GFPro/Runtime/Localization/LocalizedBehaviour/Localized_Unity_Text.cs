using UnityEngine;
using UnityEngine.UI;

namespace GFPro
{
    [AddComponentMenu("Localization/Text")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Text))]
    public class Localized_Unity_Text : LocalizedBehaviour<Text>
    {
        protected override void TranslationInternal()
        {
            if (m_Target == null) m_Target = GetComponent<Text>();
            m_Target.text = LocalizationComponent.Instance.GetString(TranslationKey);
        }
    }
}