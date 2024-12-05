using System.Collections.Generic;
using UnityEngine;

internal class ServerButtonManager : MonoBehaviour
{
    private List<ServerButtonView> buttonViews = new List<ServerButtonView>();
    private ServerButtonView selectedButtonView = null;

    public void RegisterButtonView(ServerButtonView buttonView)
    {
        buttonViews.Add(buttonView);
    }

    public void OnButtonViewSelected(ServerButtonView buttonView)
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
