using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/ShareSelectorView")]
internal class ShareSelectorView : View<ShareSelectorView>
{
    public float fadeDuration = 0.5f;
    public GameObject tapTapShare;
    public GameObject systemShare;
    private Action OnCancel;
    private Action OnSystemShare;
    private Action OnTapTapShare;

    public void SetCancelCallback(Action OnCancel) {
        this.OnCancel = OnCancel;
    }
    public void SetSystemShareEnabled(Action OnSystemShare) {
        this.OnSystemShare = OnSystemShare;
        systemShare.SetActive(true);
    }
    public void SetTapTapShareEnabled(Action OnTapTapShare) {
        this.OnTapTapShare = OnTapTapShare;
        tapTapShare.SetActive(true);
    }
    
    public void OnClose() {
        OnCancel?.Invoke();
    }
    public void OnClickSystemShare() {
        OnSystemShare?.Invoke();
    }
    public void OnClickTapTapShare() {
        OnTapTapShare?.Invoke();
    }

    protected override IEnumerator OnHide()
    {
        yield return Fade(0);
    }
    protected override IEnumerator OnShow()
    {
        yield return Fade(1);
    }

    IEnumerator Fade(float targetAlpha)
    {
        var canvasGroup = GetComponent<CanvasGroup>();
        var elapsedTime = 0.0f;
        var alpha = canvasGroup.alpha;
        while (elapsedTime < fadeDuration)
        {
            var t = elapsedTime / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(alpha, targetAlpha, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = targetAlpha;
    }
}