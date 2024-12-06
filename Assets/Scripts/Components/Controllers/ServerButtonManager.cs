using System.Collections.Generic;
using UnityEngine;

internal class ServerButtonManager : MonoBehaviour
{
    private List<ISelectableView> buttonViews = new List<ISelectableView>();
    private ISelectableView selectedButtonView = null;

    public void RegisterButtonView(ISelectableView buttonView)
    {
        buttonViews.Add(buttonView);
    }

    public void OnButtonViewSelected(ISelectableView buttonView)
    {
        if (selectedButtonView != null && selectedButtonView != buttonView)
        {
            // 隐藏之前选中的按钮的 image
            selectedButtonView.Deselect();
        }

        // 更新当前选中的按钮
        selectedButtonView = buttonView;
        selectedButtonView.Select();
    }
}
