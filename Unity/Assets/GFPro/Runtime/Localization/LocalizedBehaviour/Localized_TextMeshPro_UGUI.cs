using TMPro;
using UnityEngine;

namespace GFPro
{
    [AddComponentMenu("Localization/TextMeshPro_UGUI")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class Localized_TextMeshPro_UGUI : LocalizedBehaviour<TextMeshProUGUI>
    {
        protected override void TranslationInternal()
        {
            if (m_Target == null) m_Target = GetComponent<TextMeshProUGUI>();
            m_Target.text = LocalizationComponent.Instance.GetString(TranslationKey);
        }
    }
}