using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/LoadingView")]
internal class LoadingView : View<LoadingView>
{
    public GameObject loadingPanel;
    public Image loadingImage;
    public float delay;
    public float rotationSpeed = 80f;
    void Update()
    {
        var currentRotation = loadingImage.rectTransform.eulerAngles.z;
        var newRotation = currentRotation - rotationSpeed * Time.deltaTime;

        loadingImage.rectTransform.rotation = Quaternion.Euler(0f, 0f, newRotation);
    }

    protected override IEnumerator OnHide()
    {
        yield return null;
    }

    protected override IEnumerator OnShow()
    {
        yield return new WaitForSeconds(delay);
        loadingPanel.SetActive(true);
    }
}
