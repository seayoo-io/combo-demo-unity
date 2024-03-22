using System.Collections;
using System.Collections.Generic;
using System.IO;
using Combo;
using UnityEngine;

public static class UpdateGameViewController
{
    public static void Show(bool forceUpdate) {
        var view = UpdateGameView.Instantiate();
        if (forceUpdate) {
            view.EnableForceUpdate(() => {
                //view.Destroy();
                // 强制更新需要卡住界面
                ForceUpdate();
            });
        } else {
            view.EnableHotUpdate(() => {
                view.Destroy();
                HotUpdate();
            });
        }
        view.Show();
    }

    private static void ForceUpdate() {
        // 强制更新 且 SDK UpdateGame可用
        if (ComboSDK.IsFeatureAvailable(Feature.UPDATE_GAME)) {
            Log.D("call UpdateGame");
            ComboSDK.UpdateGame(result => {
                if (result.IsSuccess) {
                    Toast.Show("更新成功");
                    // 强制更新不回调，卡住界面
                    // UpdateGameFinishedEvent.Invoke(new UpdateGameFinishedEvent {
                    //     forceUpdate = true,
                    //     success = true
                    // });
                } else {
                    Toast.Show("更新失败：" + result.Error.ToString());
                    UpdateGameFinishedEvent.Invoke(new UpdateGameFinishedEvent {
                        forceUpdate = true,
                        success = false
                    });
                }
            });
        } else {
            Log.D("UpdateGame is not available, fallback to GetDownloadUrl");
            // 强制更新 且 SDK UpdateGame 不可用，回落到 GetDownloadUrl
            ComboSDK.GetDownloadUrl(result => {
                if(result.IsSuccess) {
                    Log.D("GameUpdateUrl: " + result.Data.downloadUrl);
                    Application.OpenURL(result.Data.downloadUrl);
                    // 强制更新不回调，卡住界面
                    // UpdateGameFinishedEvent.Invoke(new UpdateGameFinishedEvent {
                    //     forceUpdate = true,
                    //     success = true
                    // });
                } else {
                    Log.W("Failed to get game update url:" + result.Error.ToString());
                    Toast.Show("强制更新失败，UpdateGame 不可用且 GetDownloadUrl 失败：" + result.Error.ToString());
                    // UpdateGameFinishedEvent.Invoke(new UpdateGameFinishedEvent {
                    //     forceUpdate = true,
                    //     success = false
                    // });
                }
            });
        }
    }

    private static void HotUpdate() {
        Log.D("mock hot update");
        UpdateGameFinishedEvent.Invoke(new UpdateGameFinishedEvent {
            forceUpdate = false,
            success = true
        });
    }
}