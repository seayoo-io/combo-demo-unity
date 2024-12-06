using System.Collections;
using UnityEngine.UI;

[ViewPrefab("Prefabs/RoleView")]
internal class RoleView : View<RoleView>
{
    public Button confirmBtn;
    public Image image;
    void Start()
    {
       
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