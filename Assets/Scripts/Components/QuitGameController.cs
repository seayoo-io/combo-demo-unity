using System.Collections;
using System.Collections.Generic;
using Combo;
using UnityEngine;

public class QuitGameController : MonoBehaviour
{
    private static readonly string QUIT_GAME_ALERT_TAG = "QUIT_GAME_ALERT_TAG";
    private float lastPressTime = 0f;
    private float doublePressInterval = 1f;

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            // 连续两次按返回键/滑动返回手势
            if (Time.time - lastPressTime < doublePressInterval)
            {
                // 触发 QuitGame
                QuitGame();
            }
            else
            {
                lastPressTime = Time.time;
                Toast.Show("再次滑动返回退出游戏");
            }
        }
    }

    // 调用 QuitGame 退出游戏
    void QuitGame()
    {
        if (ComboSDK.IsFeatureAvailable(Feature.QUIT))
        {
            Log.I("Game Exiting");
            ComboSDK.Quit(result =>
            {
                if (result.IsSuccess)
                {
                    Application.Quit();
                }
                else
                {
                    Log.E("退出失败：" + result.Error);
                }
            });
        }
        else
        {
            UIController.Alert(UIAlertType.Singleton, "退出游戏", "确认退出游戏吗？", "确定", "取消", () => Application.Quit(), () => {});
        }
    }
}
