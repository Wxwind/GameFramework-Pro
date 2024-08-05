using UnityEngine;
using UnityEngine.UI;

namespace GameMain
{
    public class UpdateResourceForm : MonoBehaviour
    {
        [SerializeField] private Text m_DescriptionText;

        [SerializeField] private Slider m_ProgressSlider;

        private void Start()
        {
        }

        private void Update()
        {
        }

        public void SetProgress(float progress, string description)
        {
            m_ProgressSlider.value = progress;
            m_DescriptionText.text = description;
        }
    }
}