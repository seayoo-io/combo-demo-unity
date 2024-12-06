using System.Collections;
using Sentry;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/RoleSelectView")]
internal class RoleSelectView : View<RoleSelectView>
{
    public Transform parentTransform;
    public InputField nameInputField;
    public Toggle[] toggles;
    public Button confirmBtn;
    void Start()
    {
        for (int i = 0; i < toggles.Length; i++)
        {
            Toggle toggle = toggles[i];//循环遍历添加
            toggle.onValueChanged.AddListener((bool value) => OnValueChange(toggle));
        }
    }

    private void OnValueChange(Toggle toggle)
    {
        if (toggle.isOn)
        {
            Log.I(toggle.name);
        }
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