using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/UpdateGameView")]
internal class UpdateGameView : View<UpdateGameView>
{
    public GameObject hotUpdatePanel;
    public GameObject forceUpdatePanel;

    public Button hotUpdateBtn;
    public Button hotUpdateFinishedBtn;
    public Button forceUpdateBtn;

    public Text hotUpdateProgressText;
    public Slider hotUpdateProgress;

    private bool startHotUpdate = false;

    private float changeSpeed = 0.0001f;
    private float duration = 0f;
    void Awake() {
        //EnableHotUpdate();
        //EnableForceUpdate();
    }

    void FixedUpdate() {
        CheckUpdateProgress();
    }

    public void EnableHotUpdate(Action OnFinished) {
        hotUpdatePanel.SetActive(true);
        hotUpdateBtn.onClick.AddListener(() => {
            hotUpdateBtn.gameObject.SetActive(false);
            hotUpdateProgress.gameObject.SetActive(true);
            startHotUpdate = true;
        });
        hotUpdateFinishedBtn.onClick.AddListener(() => {
            OnFinished?.Invoke();
        });
    }

    public void EnableForceUpdate(Action OnFinished) {
        forceUpdatePanel.SetActive(true);
        forceUpdateBtn.onClick.AddListener(() => {
            OnFinished?.Invoke();
        });
    }

    protected override IEnumerator OnShow()
    {
        yield return null;
    }

    protected override IEnumerator OnHide()
    {
        yield return null;
    }



    private void CheckUpdateProgress()
    {
        if (!startHotUpdate) return;

        hotUpdateProgress.value += changeSpeed;

        duration -= Time.deltaTime;
        if (duration <= 0)
        {
            ChangeSpeed();
        }

        hotUpdateProgressText.text = $"{(int)Math.Round(hotUpdateProgress.value * 100)}%";

        if (hotUpdateProgress.value >= 1f) {
            startHotUpdate = false;
            hotUpdateBtn.gameObject.SetActive(false);
            hotUpdateProgress.gameObject.SetActive(false);
            hotUpdateFinishedBtn.gameObject.SetActive(true);
        }
    }

    private void ChangeSpeed()
    {
        // 在70%的概率下，选择快速增加的速度；在30%的概率下，选择慢速增加的速度
        if (UnityEngine.Random.value <= 0.5f)
        {
            changeSpeed = 0.003f;
        }
        else
        {
            changeSpeed = 0.001f;
        }

        // 选择一个新的"持续时间"，这个持续时间可以根据需要进行调整
        duration = UnityEngine.Random.Range(0.5f, 1.5f);
    }
}