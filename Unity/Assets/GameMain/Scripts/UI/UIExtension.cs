using System.Collections;
using cfg.StarForce;
using Cysharp.Threading.Tasks;
using GameFramework.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;
using UIForm = cfg.UIForm;

namespace GameMain
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
            TbUIForm tbUIForm = GameEntry.LubanConfig.Tables.TbUIForm;
            UIForm drUIForm = tbUIForm.GetOrDefault(uiFormId);
            if (drUIForm == null) return false;

            string assetName = drUIForm.AssetName;
            if (string.IsNullOrEmpty(uiGroupName)) return uiComponent.HasUIForm(assetName);

            IUIGroup uiGroup = uiComponent.GetUIGroup(uiGroupName);
            if (uiGroup == null) return false;

            return uiGroup.HasUIForm(assetName);
        }

        public static UGuiForm GetUIForm(this UIComponent uiComponent, UIFormId uiFormId, string uiGroupName = null)
        {
            return uiComponent.GetUIForm((int)uiFormId, uiGroupName);
        }

        public static UGuiForm GetUIForm(this UIComponent uiComponent, int uiFormId, string uiGroupName = null)
        {
            TbUIForm tbUIForm = GameEntry.LubanConfig.Tables.TbUIForm;
            UIForm drUIForm = tbUIForm.GetOrDefault(uiFormId);


            if (drUIForm == null) return null;

            string assetName = drUIForm.AssetName;
            UnityGameFramework.Runtime.UIForm uiForm = null;
            if (string.IsNullOrEmpty(uiGroupName))
            {
                uiForm = uiComponent.GetUIForm(assetName);
                if (uiForm == null) return null;

                return (UGuiForm)uiForm.Logic;
            }

            IUIGroup uiGroup = uiComponent.GetUIGroup(uiGroupName);
            if (uiGroup == null) return null;

            uiForm = (UnityGameFramework.Runtime.UIForm)uiGroup.GetUIForm(assetName);
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
            TbUIForm tbUIForm = GameEntry.LubanConfig.Tables.TbUIForm;
            UIForm drUIForm = tbUIForm.GetOrDefault(uiFormId);
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

            return await uiComponent.OpenUIForm(assetName, drUIForm.UIGroupName, Constant.AssetPriority.UIFormAsset,
                drUIForm.PauseCoveredUIForm, userData);
        }

        public static void OpenDialog(this UIComponent uiComponent, DialogParams dialogParams)
        {
            if (((ProcedureBase)GameEntry.Procedure.CurrentProcedure).UseNativeDialog)
                OpenNativeDialog(dialogParams);
            else
                // TODO: 支持dialogParams
                uiComponent.OpenUIForm(UIFormId.DialogForm, dialogParams);
        }

        private static void OpenNativeDialog(DialogParams dialogParams)
        {
            // TODO：这里应该弹出原生对话框，先简化实现为直接按确认按钮
            if (dialogParams.OnClickConfirm != null) dialogParams.OnClickConfirm(dialogParams.UserData);
        }
    }
}