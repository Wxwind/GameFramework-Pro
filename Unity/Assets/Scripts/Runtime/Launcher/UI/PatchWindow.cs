using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class PatchWindow : MonoBehaviour
    {
        private readonly List<MessageBox> _msgBoxList = new();

        // UGUI相关
        private GameObject _messageBoxObj;
        private Slider     _slider;
        private Text       _tips;


        private void Awake()
        {
            _slider = transform.Find("UIWindow/Slider").GetComponent<Slider>();
            _tips = transform.Find("UIWindow/Slider/txt_tips").GetComponent<Text>();
            _tips.text = "Initializing the game world !";
            _messageBoxObj = transform.Find("UIWindow/MessgeBox").gameObject;
            _messageBoxObj.SetActive(false);
        }


        /// <summary>
        ///     显示对话框
        /// </summary>
        public void ShowMessageBox(string content, Action ok)
        {
            // 尝试获取一个可用的对话框
            MessageBox msgBox = null;
            for (int i = 0; i < _msgBoxList.Count; i++)
            {
                var item = _msgBoxList[i];
                if (item.ActiveSelf == false)
                {
                    msgBox = item;
                    break;
                }
            }

            // 如果没有可用的对话框，则创建一个新的对话框
            if (msgBox == null)
            {
                msgBox = new MessageBox();
                var cloneObject = Instantiate(_messageBoxObj, _messageBoxObj.transform.parent);
                msgBox.Create(cloneObject);
                _msgBoxList.Add(msgBox);
            }

            // 显示对话框
            msgBox.Show(content, ok);
        }

        public void ShowToolTip(string content)
        {
            _tips.text = content;
        }

        public void ShowDownloadProgress(float value, string text)
        {
            _slider.value = value;
            _tips.text = text;
        }

        /// <summary>
        ///     对话框封装类
        /// </summary>
        private class MessageBox
        {
            private Button     _btnOK;
            private Action     _clickOK;
            private GameObject _cloneObject;
            private Text       _content;

            public bool ActiveSelf => _cloneObject.activeSelf;

            public void Create(GameObject cloneObject)
            {
                _cloneObject = cloneObject;
                _content = cloneObject.transform.Find("txt_content").GetComponent<Text>();
                _btnOK = cloneObject.transform.Find("btn_ok").GetComponent<Button>();
                _btnOK.onClick.AddListener(OnOk);
            }

            public void Show(string content, Action clickOK)
            {
                _content.text = content;
                _clickOK = clickOK;
                _cloneObject.SetActive(true);
                _cloneObject.transform.SetAsLastSibling();
            }

            public void Hide()
            {
                _content.text = string.Empty;
                _clickOK = null;
                _cloneObject.SetActive(false);
            }

            private void OnOk()
            {
                _clickOK?.Invoke();
                Hide();
            }
        }
    }
}