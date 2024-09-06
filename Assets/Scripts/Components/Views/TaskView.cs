using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/TaskView")]
internal class TaskView : View<TaskView>
{
    public InputField placementIdField;
    public Button preloadAdBtn;
    public Button showAdBtn;
    public Button cancelBtn;
    private Action OnCancel;
    public InputField scenarioIdField; 

    void Awake()
    {
        cancelBtn.onClick.AddListener(OnClickCancelBtn);
        ButtonManager.SetButtonEnabledByType(preloadAdBtn, ButtonType.PreloadAdButton);
        ButtonManager.SetButtonEnabledByType(showAdBtn, ButtonType.ShowAdButton);
    }

    void OnDestroy()
    {
        cancelBtn.onClick.RemoveListener(OnClickCancelBtn);
    }

    public void OnPreloadAd(){
        PreloadAdEvent.Invoke(new PreloadAdEvent {
            placementId = placementIdField.text,
            scenarioId = scenarioIdField.text
        });
    }

    public void OnShowAd(){
        ShowAdEvent.Invoke(new ShowAdEvent {
            placementId = placementIdField.text,
            scenarioId = scenarioIdField.text
        });
    }

    void OnClickCancelBtn()
    {
        OnCancel.Invoke();
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
