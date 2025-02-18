using System;
using System.Collections;
using UnityEngine.UI;

[ViewPrefab("Prefabs/ChangeTimeView")]
internal class ChangeTimeView : View<ChangeTimeView>
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
        closeBtn.onClick.AddListener(() => { Destroy(); });
    }

    void OnDestroy()
    {
        EventSystem.UnRegister(this);
        confirmBtn.onClick.RemoveListener(Confirm);
        closeBtn.onClick.RemoveListener(() => { Destroy(); });
    }

    [EventSystem.BindEvent]
    public void ChangeRoleCreateTime(UpdateTimeEvent evt)
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
        ChangeTimeEvent.Invoke(new ChangeTimeEvent{
            unixTime = unixTimestamp
        });
        Destroy();
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