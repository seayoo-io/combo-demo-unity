using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/ErrorTrackView")]
internal class ErrorTrackView : View<ErrorTrackView>
{
    public Button captureBtn;
    public Button crashBtn;
    public Button reportDataBtn;
    public Button cancelBtn;
    private Action OnCapture;
    private Action OnCrash;
    private Action OnData;
    private Action OnCancel; 
    void Awake()
    {
        captureBtn.onClick.AddListener(OnCaptureConfirmBtn);
        crashBtn.onClick.AddListener(OnCrashConfirmBtn);
        reportDataBtn.onClick.AddListener(OnDataConfirmBtn);
        cancelBtn.onClick.AddListener(OnClickCancelBtn);
        ButtonManager.SetButtonEnabledByType(captureBtn, ButtonType.SentryCaptureButton);
        ButtonManager.SetButtonEnabledByType(crashBtn, ButtonType.SentryCrashButton);
        ButtonManager.SetButtonEnabledByType(reportDataBtn, ButtonType.DataReportButton);
    }

    void OnDestroy()
    {
        captureBtn.onClick.RemoveListener(OnCaptureConfirmBtn);
        crashBtn.onClick.RemoveListener(OnCrashConfirmBtn);
        reportDataBtn.onClick.RemoveListener(OnDataConfirmBtn);
        cancelBtn.onClick.RemoveListener(OnClickCancelBtn);
    }

    void OnCaptureConfirmBtn()
    {
        OnCapture.Invoke();
    }

    void OnCrashConfirmBtn()
    {
        OnCrash.Invoke();
    }

    void OnDataConfirmBtn()
    {
        OnData.Invoke();
    }

    void OnClickCancelBtn()
    {
        OnCancel.Invoke();
    }

    public void SetCaptureCallback(Action OnCapture)
    {
        this.OnCapture = OnCapture;
    }

    public void SetCrashCallback(Action OnCrash)
    {
        this.OnCrash = OnCrash;
    }

    public void SetDataCallback(Action OnData)
    {
        this.OnData = OnData;
    }

    public void SetCancelCallback(Action OnCancel)
    {
        this.OnCancel = OnCancel;
    }

    protected override IEnumerator OnHide()
    {
        yield return null;
    }

    protected override IEnumerator OnShow()
    {
        yield return null;
    }
}
