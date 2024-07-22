using System;
using System.Collections.Generic;
using GameFramework.Event;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;
using GameEntry = StarForce.GameEntry;

public class PatchWindow : MonoBehaviour
{
    /// <summary>
    /// 对话框封装类
    /// </summary>
    private class MessageBox
    {
        private GameObject _cloneObject;
        private Text       _content;
        private Button     _btnOK;
        private Action     _clickOK;

        public bool ActiveSelf => _cloneObject.activeSelf;

        public void Create(GameObject cloneObject)
        {
            _cloneObject = cloneObject;
            _content = cloneObject.transform.Find("txt_content").GetComponent<Text>();
            _btnOK = cloneObject.transform.Find("btn_ok").GetComponent<Button>();
            _btnOK.onClick.AddListener(OnClickYes);
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

        private void OnClickYes()
        {
            _clickOK?.Invoke();
            Hide();
        }
    }


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

        GameEntry.Event.Subscribe(InitializeFailedEventArgs.EventId, OnHandleEventMessage);
        GameEntry.Event.Subscribe(PatchStatesChangeEventArgs.EventId, OnHandleEventMessage);
        GameEntry.Event.Subscribe(FoundUpdateFilesEventArgs.EventId, OnHandleEventMessage);
        GameEntry.Event.Subscribe(DownloadProgressUpdateEventArgs.EventId, OnHandleEventMessage);
        GameEntry.Event.Subscribe(PackageVersionUpdateFailedEventArgs.EventId, OnHandleEventMessage);
        GameEntry.Event.Subscribe(PatchManifestUpdateFailedEventArgs.EventId, OnHandleEventMessage);
        GameEntry.Event.Subscribe(WebFileDownloadFailedEventArgs.EventId, OnHandleEventMessage);
    }

    private void OnDestroy()
    {
        GameEntry.Event.Unsubscribe(InitializeFailedEventArgs.EventId, OnHandleEventMessage);
        GameEntry.Event.Unsubscribe(PatchStatesChangeEventArgs.EventId, OnHandleEventMessage);
        GameEntry.Event.Unsubscribe(FoundUpdateFilesEventArgs.EventId, OnHandleEventMessage);
        GameEntry.Event.Unsubscribe(DownloadProgressUpdateEventArgs.EventId, OnHandleEventMessage);
        GameEntry.Event.Unsubscribe(PackageVersionUpdateFailedEventArgs.EventId, OnHandleEventMessage);
        GameEntry.Event.Unsubscribe(PatchManifestUpdateFailedEventArgs.EventId, OnHandleEventMessage);
        GameEntry.Event.Unsubscribe(WebFileDownloadFailedEventArgs.EventId, OnHandleEventMessage);
    }

    /// <summary>
    /// 接收事件
    /// </summary>
    private void OnHandleEventMessage(object sender, GameEventArgs message)
    {
        if (message is InitializeFailedEventArgs)
        {
            //  Action callback = () => { UserTryInitialize.SendEventMessage(); };
            //   ShowMessageBox($"Failed to initialize package !", callback);
        }
        else if (message is PatchStatesChangeEventArgs)
        {
            var msg = message as PatchStatesChangeEventArgs;
            _tips.text = msg.Tips;
        }
        else if (message is FoundUpdateFilesEventArgs)
        {
            var msg = message as FoundUpdateFilesEventArgs;
            Action callback = () => { GameEntry.Event.Fire(this, UserBeginDownloadWebFilesEventArgs.Create()); };
            var sizeMB = msg.TotalSizeBytes / 1048576f;
            sizeMB = Mathf.Clamp(sizeMB, 0.1f, float.MaxValue);
            var totalSizeMB = sizeMB.ToString("f1");
            ShowMessageBox($"Found update patch files, Total count {msg.TotalCount} Total szie {totalSizeMB}MB", callback);
        }
        else if (message is DownloadProgressUpdateEventArgs)
        {
            var msg = message as DownloadProgressUpdateEventArgs;
            _slider.value = (float)msg.CurrentDownloadCount / msg.TotalDownloadCount;
            var currentSizeMB = (msg.CurrentDownloadSizeBytes / 1048576f).ToString("f1");
            var totalSizeMB = (msg.TotalDownloadSizeBytes / 1048576f).ToString("f1");
            _tips.text = $"{msg.CurrentDownloadCount}/{msg.TotalDownloadCount} {currentSizeMB}MB/{totalSizeMB}MB";
        }
        else if (message is PackageVersionUpdateFailedEventArgs)
        {
            Action callback = () => { GameEntry.Event.Fire(this, UserTryUpdatePackageVersionEventArgs.Create()); };
            ShowMessageBox($"Failed to update static version, please check the network status.", callback);
        }
        else if (message is PatchManifestUpdateFailedEventArgs)
        {
            Action callback = () => { GameEntry.Event.Fire(this, UserTryUpdatePatchManifestEventArgs.Create()); };
            ShowMessageBox($"Failed to update patch manifest, please check the network status.", callback);
        }
        else if (message is WebFileDownloadFailedEventArgs)
        {
            var msg = message as WebFileDownloadFailedEventArgs;
            Action callback = () => { GameEntry.Event.Fire(this, UserTryDownloadWebFilesEventArgs.Create()); };
            ShowMessageBox($"Failed to download file : {msg.FileName}", callback);
        }
        else
        {
            throw new NotImplementedException($"{message.GetType()}");
        }
    }

    /// <summary>
    /// 显示对话框
    /// </summary>
    private void ShowMessageBox(string content, Action ok)
    {
        // 尝试获取一个可用的对话框
        MessageBox msgBox = null;
        for (var i = 0; i < _msgBoxList.Count; i++)
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
}