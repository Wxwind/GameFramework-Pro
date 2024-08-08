using System.Collections;
using Cysharp.Threading.Tasks;
using GameFramework.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace Game
{
    public static class UIExtension
    {
        public static IEnumerator FadeToAlpha(this CanvasGroup canvasGroup, float alpha, float duration)
        {
            float time = 0f;
            float originalAlpha = canvasGroup.alpha;
            while (time < duration)
            {
                time += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(originalAlpha, alpha, time / duration);
                yield return new WaitForEndOfFrame();
            }

            canvasGroup.alpha = alpha;
        }

        public static IEnumerator SmoothValue(this Slider slider, float value, float duration)
        {
            float time = 0f;
            float originalValue = slider.value;
            while (time < duration)
            {
                time += Time.deltaTime;
                slider.value = Mathf.Lerp(originalValue, value, time / duration);
                yield return new WaitForEndOfFrame();
            }

            slider.value = value;
        }

        public static bool HasUIForm(this UIComponent uiComponent, UIFormId uiFormId, string uiGroupName = null)
        {
            return uiComponent.HasUIForm((int)uiFormId, uiGroupName);
        }

        public static bool HasUIForm(this UIComponent uiComponent, int uiFormId, string uiGroupName = null)
        {
            var tbUIForm = GameEntry.LubanDataTable.Tables.TbUIForm;
            var drUIForm = tbUIForm.GetOrDefault(uiFormId);
            if (drUIForm == null) return false;

            string assetName = drUIForm.AssetName;
            if (string.IsNullOrEmpty(uiGroupName)) return uiComponent.HasUIForm(assetName);

            var uiGroup = uiComponent.GetUIGroup(uiGroupName);
            if (uiGroup == null) return false;

            return uiGroup.HasUIForm(assetName);
        }

        public static UGuiForm GetUIForm(this UIComponent uiComponent, UIFormId uiFormId, string uiGroupName = null)
        {
            return uiComponent.GetUIForm((int)uiFormId, uiGroupName);
        }

        public static UGuiForm GetUIForm(this UIComponent uiComponent, int uiFormId, string uiGroupName = null)
        {
            var tbUIForm = GameEntry.LubanDataTable.Tables.TbUIForm;
            var drUIForm = tbUIForm.GetOrDefault(uiFormId);


            if (drUIForm == null) return null;

            string assetName = drUIForm.AssetName;
            UIForm uiForm = null;
            if (string.IsNullOrEmpty(uiGroupName))
            {
                uiForm = uiComponent.GetUIForm(assetName);
                if (uiForm == null) return null;

                return (UGuiForm)uiForm.Logic;
            }

            var uiGroup = uiComponent.GetUIGroup(uiGroupName);
            if (uiGroup == null) return null;

            uiForm = (UIForm)uiGroup.GetUIForm(assetName);
            if (uiForm == null) return null;

            return (UGuiForm)uiForm.Logic;
        }

        public static void CloseUIForm(this UIComponent uiComponent, UGuiForm uiForm)
        {
            uiComponent.CloseUIForm(uiForm.UIForm);
        }

        public static UniTask<IUIForm> OpenUIForm(this UIComponent uiComponent, UIFormId uiFormId, object userData = null)
        {
            return uiComponent.OpenUIForm((int)uiFormId, userData);
        }

        public static async UniTask<IUIForm> OpenUIForm(this UIComponent uiComponent, int uiFormId, object userData = null)
        {
            var tbUIForm = GameEntry.LubanDataTable.Tables.TbUIForm;
            var drUIForm = tbUIForm.GetOrDefault(uiFormId);
            if (drUIForm == null)
            {
                Log.Warning("Can not load UI form '{0}' from data table.", uiFormId.ToString());
                return null;
            }

            string assetName = drUIForm.AssetName;
            if (!drUIForm.AllowMultiInstance)
            {
                if (uiComponent.IsLoadingUIForm(assetName)) return null;

                if (uiComponent.HasUIForm(assetName)) return null;
            }

            return await uiComponent.OpenUIForm(assetName, drUIForm.UIGroupName,
                drUIForm.PauseCoveredUIForm, userData);
        }

        public static void OpenDialog(this UIComponent uiComponent, DialogParams dialogParams)
        {
            uiComponent.OpenUIForm(UIFormId.DialogForm, dialogParams);
        }
    }
}