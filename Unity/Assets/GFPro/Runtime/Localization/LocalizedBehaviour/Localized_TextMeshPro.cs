using TMPro;
using UnityEngine;

namespace GFPro
{
    [AddComponentMenu("Localization/TextMeshPro")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class Localized_TextMeshPro : LocalizedBehaviour<TextMeshPro>
    {
        protected override void TranslationInternal()
        {
            if (m_Target == null) m_Target = GetComponent<TextMeshPro>();
            m_Target.text = LocalizationComponent.Instance.GetString(TranslationKey);
        }
    }
}