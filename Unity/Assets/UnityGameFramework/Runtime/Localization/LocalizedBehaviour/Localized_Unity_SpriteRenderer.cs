using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GFPro
{
    [AddComponentMenu("Localization/SpriteRenderer")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public class Localized_Unity_SpriteRenderer : LocalizedBehaviour<SpriteRenderer>
    {
        protected override void TranslationInternal()
        {
            LoadInternal().Forget();
        }

        private async UniTaskVoid LoadInternal()
        {
            if (m_Target == null) m_Target = GetComponent<SpriteRenderer>();
            var sprite = await GameEntry.GetComponent<ResourceComponent>().LoadAssetAsync<Sprite>(TranslationKey);
            m_Target.sprite = sprite;
        }
    }
}