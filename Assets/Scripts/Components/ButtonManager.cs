using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UI;

public enum ButtonType {
    LogoutButton,
    ClearButton,
    SentryCaptureButton,
    SentryCrashButton,
    DataReportButton,
    LinkShareButton,
    OnlineShareButton,
    LocalShareButton,
    PreloadAdButton,
    ShowAdButton,
    PurchaseButton,
}

public static class ButtonManager
{
    #if UNITY_STANDALONE
        private static Dictionary<ButtonType, bool> enableButton = new Dictionary<ButtonType, bool>{
            {ButtonType.LogoutButton, true}, 
            {ButtonType.ClearButton, true}, 
            {ButtonType.SentryCaptureButton, true}, 
            {ButtonType.SentryCrashButton, true},
            {ButtonType.DataReportButton, false},
            {ButtonType.LinkShareButton, false},
            {ButtonType.OnlineShareButton, false},
            {ButtonType.LocalShareButton, false},
            {ButtonType.PreloadAdButton, false},
            {ButtonType.ShowAdButton, false},
            {ButtonType.PurchaseButton, true}};
    #elif UNITY_ANDROID
        private static Dictionary<ButtonType, bool> enableButton = new Dictionary<ButtonType, bool>{
            {ButtonType.LogoutButton, true}, 
            {ButtonType.ClearButton, false}, 
            {ButtonType.SentryCaptureButton, true}, 
            {ButtonType.SentryCrashButton, true},
            {ButtonType.DataReportButton, false},
            {ButtonType.LinkShareButton, true},
            {ButtonType.OnlineShareButton, true},
            {ButtonType.LocalShareButton, true},
            {ButtonType.PreloadAdButton, true},
            {ButtonType.ShowAdButton, true},
            {ButtonType.PurchaseButton, true}};
    #elif UNITY_IOS
        private static Dictionary<ButtonType, bool> enableButton = new Dictionary<ButtonType, bool>{
            {ButtonType.LogoutButton, true}, 
            {ButtonType.ClearButton, false}, 
            {ButtonType.SentryCaptureButton, true}, 
            {ButtonType.SentryCrashButton, true},
            {ButtonType.DataReportButton, false},
            {ButtonType.LinkShareButton, true},
            {ButtonType.OnlineShareButton, true},
            {ButtonType.LocalShareButton, true},
            {ButtonType.PreloadAdButton, true},
            {ButtonType.ShowAdButton, true},
            {ButtonType.PurchaseButton, true}};
    #endif

    private static float brightness = 0.5f;

    public static void SetButtonEnabledByType(Button button, ButtonType buttonType)
    {
        bool interactable;
        enableButton.TryGetValue(buttonType, out interactable);
        button.interactable = interactable;
        UpdateButtonAndChildrenColors(button);
    }

    private static void UpdateButtonAndChildrenColors(Button button)
    {
        Image[] images = button.GetComponentsInChildren<Image>(true);
        foreach (var image in images)
        {
            if (image.transform == button.transform)
            {
                continue;
            }
            Color currentColor = image.color;
            Color newColor = new Color(currentColor.r * brightness, currentColor.g * brightness, currentColor.b * brightness, currentColor.a);
            image.color = button.interactable ? currentColor : newColor;
        }
    }

}
