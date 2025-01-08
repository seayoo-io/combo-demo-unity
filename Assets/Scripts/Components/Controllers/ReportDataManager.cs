using System;
using Combo;
using UnityEngine;

public enum ReportType
{
    PromoPseudoPurchase,
    Login,
    ActiveValue
}

public class ReportDataManager : MonoBehaviour
{
    private string time;
    private LoginReportEvent loginReport;
    private ActiveValueReportEvent activeValueReport;
    private ReportType currentReport;
    void Start()
    {
        EventSystem.Register(this);
    }

    void OnDestroy()
    {
        EventSystem.UnRegister(this);
    }

    [EventSystem.BindEvent]
    void HandlePromoPseudoPurchaseEvent(PromoPseudoPurchaseEvent evt)
    {
        string amount = evt.amount;
        if (string.IsNullOrEmpty(evt.amount))
        {
            amount = "0";
        }
        int result;
        if (int.TryParse(amount, out result))
        {
            OnPromoPseudoPurchase(result);
        }
        else
        {
            Toast.Show("转换失败或数字超出 int 的范围");
        }
    }

    [EventSystem.BindEvent]
    void HandleLoginEvent(LoginEvent evt)
    {
        currentReport = ReportType.Login;
        ChangeTimeView.Instantiate();
        var roleInfo = PlayerController.GetRoleInfo(PlayerController.GetPlayer());
        loginReport = new LoginReportEvent
        {
            type = "role_login",
            comboId = ComboSDK.GetLoginInfo().comboId,
            serverId = roleInfo.serverId,
            roleId = roleInfo.roleId,
            roleLevel = roleInfo.roleLevel
        };
    }

    [EventSystem.BindEvent]
    void HandleActiveValueEvent(ActiveValueEvent evt)
    {
        currentReport = ReportType.ActiveValue;
        string amount = evt.value;
        if (string.IsNullOrEmpty(evt.value))
        {
            amount = "0";
        }
        int result;
        if (int.TryParse(amount, out result))
        {
            ChangeTimeView.Instantiate();
            var roleInfo = PlayerController.GetRoleInfo(PlayerController.GetPlayer());
            activeValueReport = new ActiveValueReportEvent
            {
                type = "role_activity_points",
                comboId = ComboSDK.GetLoginInfo().comboId,
                serverId = roleInfo.serverId,
                roleId = roleInfo.roleId,
                activityPoints = result,
                pointsChanged = result
            };
        }
        else
        {
            Toast.Show("转换失败或数字超出 int 的范围");
        }
    }

    [EventSystem.BindEvent]
    public void ChangeTime(ChangeTimeEvent evt)
    {
        time = ConvertUnixTimeToIso8601(evt.unixTime);
        switch (currentReport)
        {
            case ReportType.Login:
                loginReport.time = time;
                GameClient.ReportEvent(loginReport, (error)=>{});
                break;
            case ReportType.ActiveValue:
                activeValueReport.time = time;
                GameClient.ReportEvent(activeValueReport, (error)=>{});
                break;
        }
    }

    public void OnPromoPseudoPurchase(int amount)
    {
        PromoPseudoPurchaseOptions opts = new PromoPseudoPurchaseOptions()
        {
            Amount = amount,
        };
        ComboSDK.PromoPseudoPurchase(opts);
    }

    public static string ConvertUnixTimeToIso8601(long unixTime)
    {
        // 将 Unix 时间戳转换为 DateTime
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTime);

        // 获取东八区的时区信息
        TimeZoneInfo chinaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
        
        // 转换为东八区的时间
        DateTimeOffset chinaTime = TimeZoneInfo.ConvertTime(dateTimeOffset, chinaTimeZone);

        // 格式化为 ISO 8601 字符串
        return chinaTime.ToString("yyyy-MM-ddTHH:mm:sszzz");
    }
}
