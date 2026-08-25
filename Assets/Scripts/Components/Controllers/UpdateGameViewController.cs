using System.Collections;
using System.Collections.Generic;
using System.IO;
using Combo;

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
        // 强制更新：改用聚合更新接口 UpdateApp，由 SDK 内部判断当前发行版本走渠道更新
        // 还是跳转浏览器/应用商店下载，不再自行判断 IsFeatureAvailable(UPDATE_GAME)
        Log.D("call UpdateApp");
#if !UNITY_WEBGL
        ComboSDK.UpdateApp(result => {
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
#else
        // WebGL 平台不支持 UpdateApp 接口（编译期即不可用），网页无需强更 App
        Toast.Show("WebGL 平台不支持强制更新");
        UpdateGameFinishedEvent.Invoke(new UpdateGameFinishedEvent {
            forceUpdate = true,
            success = false
        });
#endif
    }

    private static void HotUpdate() {
        Log.D("mock hot update");
        UpdateGameFinishedEvent.Invoke(new UpdateGameFinishedEvent {
            forceUpdate = false,
            success = true
        });
    }
}