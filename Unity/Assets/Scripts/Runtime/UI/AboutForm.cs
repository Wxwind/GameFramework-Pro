using GFPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class AboutForm : UGuiForm
    {
        [SerializeField] private RectTransform m_Transform;

        [SerializeField] private float m_ScrollSpeed = 1f;

        private float m_InitPosition;


        protected override void OnInit()
        {
            base.OnInit();

            var canvasScaler = GetComponentInParent<CanvasScaler>();
            if (canvasScaler == null)
            {
                Log.Warning("Can not find CanvasScaler component.");
                return;
            }

            m_InitPosition = -0.5f * canvasScaler.referenceResolution.x * Screen.height / Screen.width;
        }


        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            m_Transform.SetLocalPositionY(m_InitPosition);

            // 换个音乐
            GameEntry.Sound.PlayMusic(3);
        }


        protected override void OnClose(bool isShutdown)
        {
            base.OnClose(isShutdown);

            // 还原音乐
            GameEntry.Sound.PlayMusic(1);
        }


        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            m_Transform.AddLocalPositionY(m_ScrollSpeed * elapseSeconds);
            if (m_Transform.localPosition.y > m_Transform.sizeDelta.y - m_InitPosition)
                m_Transform.SetLocalPositionY(m_InitPosition);
        }
    }
}