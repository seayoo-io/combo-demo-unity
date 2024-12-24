using System;
using UnityEngine;
using UnityEngine.UI;

public class ChangeRoleTimeController : MonoBehaviour
{
    public Button confirmBtn;
    public Button closeBtn;
    public Text YMDText;
    public Text TimeText;
    private DateTime currentTime;

    void Awake()
    {
        EventSystem.Register(this);
        currentTime = DateTime.Now;
        YMDText.text = $"{currentTime.Year}年{currentTime.Month}月{currentTime.Day}日";
        TimeText.text = $"{currentTime.Hour}时{currentTime.Minute}分{currentTime.Second}秒";
        confirmBtn.onClick.AddListener(Confirm);
        closeBtn.onClick.AddListener(() => { gameObject.SetActive(false); });
    }

    void OnDestroy()
    {
        EventSystem.UnRegister(this);
        confirmBtn.onClick.RemoveListener(Confirm);
        closeBtn.onClick.RemoveListener(() => { gameObject.SetActive(false); });
    }

    [EventSystem.BindEvent]
    public void ChangeRoleCreateTime(UpdateRoleCreateTimeEvent evt)
    {
        currentTime = evt.changeTime;
        YMDText.text = $"{currentTime.Year}年{currentTime.Month}月{currentTime.Day}日";
        TimeText.text = $"{currentTime.Hour}时{currentTime.Minute}分{currentTime.Second}秒";
    }

    public void Confirm()
    {
        // 创建 Unix 纪元时间
        DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // 计算时间差并转换为秒数
        long unixTimestamp = Convert.ToInt64((currentTime.ToUniversalTime() - unixEpoch).TotalSeconds);
        ChangeRoleCreateTimeEvent.Invoke(new ChangeRoleCreateTimeEvent{
            unixTime = unixTimestamp
        });
        gameObject.SetActive(false);
    }
}