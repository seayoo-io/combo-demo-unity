using System.Collections;
using Combo;
using UnityEngine.UI;

[ViewPrefab("Prefabs/LanguageView")]
internal class LanguageView : View<LanguageView>
{
    public Text currentLanguage;
    public Button changeLanguageBtn;
    public Button closeBtn;
    public Dropdown dropdown;
    private LanguagePreference languagePreference = LanguagePreference.ChineseSimplified;

    void Start()
    {
        currentLanguage.text = ComboSDK.LanguageCode;
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
                languagePreference = LanguagePreference.ChineseSimplified;
                break;
            case 1:
                languagePreference = LanguagePreference.English;
                break;
            case 2:
            default:
                languagePreference = LanguagePreference.FollowSystem;
                break;
        }
    }

    public void SetLanguagePreference()
    {
        ComboSDK.LanguagePreference = languagePreference;
        currentLanguage.text = ComboSDK.LanguageCode;
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