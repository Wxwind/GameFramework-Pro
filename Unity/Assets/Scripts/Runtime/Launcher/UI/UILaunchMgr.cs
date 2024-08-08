using System;
using UnityEngine;
using UnityGameFramework.Runtime;
using Object = UnityEngine.Object;

namespace Game
{
    public static class UILaunchMgr
    {
        private static PatchWindow _uiRoot;

        public static void Initialize()
        {
            if (_uiRoot != null) return;
            var go = Resources.Load<GameObject>("PatchWindow");
            _uiRoot = Object.Instantiate(go).GetComponent<PatchWindow>();
            if (_uiRoot == null) Log.Error("fail to find UIRoot");
        }

        public static void ShowMessageBox(string content, Action onOk = null, Action onCancel = null)
        {
            _uiRoot.ShowMessageBox(content, onOk);
        }

        public static void ShowTip(string content)
        {
            _uiRoot.ShowToolTip(content);
        }

        public static void ShowDownloadProgress(float value, string text)
        {
            _uiRoot.ShowDownloadProgress(value, text);
        }

        public static void DestroySelf()
        {
            if (_uiRoot == null) return;
            Object.Destroy(_uiRoot.gameObject);
            _uiRoot = null;
        }
    }
}