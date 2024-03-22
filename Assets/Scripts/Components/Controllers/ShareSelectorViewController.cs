using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Combo;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ShareSelectorViewController
{
    public static void Show()
    {
        var view = ShareSelectorView.Instantiate();
        view.SetCancelCallback(() =>
        {
            view.Hide();
        });

        var availableShares = ComboSDK.GetAvailableShareTargets();

        if (availableShares.Contains(ShareTarget.SYSTEM))
        {
            view.SetSystemShareEnabled(() =>
            {
                ShareOptionViewController.Show(ShareTarget.SYSTEM);
                view.Hide();
            });
        }

        if (availableShares.Contains(ShareTarget.TAPTAP))
        {
            view.SetTapTapShareEnabled(() =>
            {
                ShareOptionViewController.Show(ShareTarget.TAPTAP);
                view.Hide();
            });
        }

        view.Show(new ViewCallbacks { AfterHide = () => view.Destroy() });
    }
}
