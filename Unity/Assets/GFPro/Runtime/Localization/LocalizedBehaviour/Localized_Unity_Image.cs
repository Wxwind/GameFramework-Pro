using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace GFPro
{
    [AddComponentMenu("Localization/Image")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public class Localized_Unity_Image : LocalizedBehaviour<Image>
    {
        protected override void TranslationInternal()
        {
            LoadInternal().Forget();
        }

        private async UniTaskVoid LoadInternal()
        {
            if (m_Target == null) m_Target = GetComponent<Image>();
            var sprite = await ResourceComponent.Instance.LoadAssetAsync<Sprite>(TranslationKey);
            m_Target.sprite = sprite;
        }
    }
}