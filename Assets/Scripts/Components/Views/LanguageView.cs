using System.Collections;
using Combo;
using UnityEngine.UI;

[ViewPrefab("Prefabs/LanguageView")]
internal class LanguageView : View<LanguageView>
{
    public Text currentLanguage;
    public Text currentLanguagePre;
    public Button changeLanguageBtn;
    public Button closeBtn;
    public Dropdown dropdown;
    private LanguagePreference languagePreference = LanguagePreference.ChineseSimplified;

    void Start()
    {
        currentLanguage.text = ComboSDK.LanguageCode;
        currentLanguagePre.text = ComboSDK.LanguagePreference.ToString();
        SetDropdownOption((int)ComboSDK.LanguagePreference);
        dropdown.onValueChanged.AddListener(DropdownChenge);
        changeLanguageBtn.onClick.AddListener(SetLanguagePreference);
        closeBtn.onClick.AddListener(Destroy);
    }

    void OnDestroy()
    {
        dropdown.onValueChanged.RemoveListener(DropdownChenge);
        changeLanguageBtn.onClick.RemoveListener(SetLanguagePreference);
        closeBtn.onClick.RemoveListener(Destroy);
    }

    public void DropdownChenge(int index)
    {
        switch (index)
        {
            case 0:
                languagePreference = LanguagePreference.FollowSystem;
                break;
            case 1:
                languagePreference = LanguagePreference.ChineseSimplified;
                break;
            case 2:
            default:
                languagePreference = LanguagePreference.English;
                break;
        }
    }

    public void SetLanguagePreference()
    {
        ComboSDK.LanguagePreference = languagePreference;
        currentLanguage.text = ComboSDK.LanguageCode;
        currentLanguagePre.text = ComboSDK.LanguagePreference.ToString();
    }

    private void SetDropdownOption(int index)
    {
        if (dropdown != null && index >= 0 && index < dropdown.options.Count)
        {
            dropdown.value = index;
            languagePreference = (LanguagePreference)index;
            // 刷新以确保UI更新
            dropdown.RefreshShownValue();
        }
    }

    // private string GetCurrentLanguagePre()
    // {
    //     switch (ComboSDK.LanguagePreference)
    //     {
    //         case 0:
    //             return "FollowSystem";
    //         case 1:
    //             return "ChineseSimplified";
    //         case 2:
    //             return 
    //     }
    // }

    protected override IEnumerator OnHide()
    {
        yield return null;
    }

    protected override IEnumerator OnShow()
    {
        yield return null;
    }
}