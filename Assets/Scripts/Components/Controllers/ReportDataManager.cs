using System;
using System.Collections.Generic;
using Combo;
using UnityEngine;

public enum ReportType
{
    PromoPseudoPurchase,
    Login,
    ActiveValue,
    RoundEnd,
    BattleResult,
    CardDraw
}

public class ReportDataManager : MonoBehaviour
{
    private string time;
    private LoginReportEvent loginReport;
    private ActiveValueReportEvent activeValueReport;
    private RoundEndReportEvent roundEndReport;
    private BattleResultReportEvent battleResultReport;
    private CardDrawReportEvent cardDrawReport;
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
    void HandleRoundEndEvent(RoundEndEvent evt)
    {
        currentReport = ReportType.RoundEnd;
        ChangeTimeView.Instantiate();
        var roleInfo = PlayerController.GetRoleInfo(PlayerController.GetPlayer());
        roundEndReport = new RoundEndReportEvent
        {
            type = "round_end",
            comboId = ComboSDK.GetLoginInfo().comboId,
            serverId = roleInfo.serverId,
            roleId = roleInfo.roleId,
            roleLevel = roleInfo.roleLevel,
            roleName = roleInfo.roleName,
            accountId = ComboSDK.GetLoginInfo().comboId,
            os = SystemInfo.operatingSystem,
            distro = ComboSDK.GetDistro(),
            variant = ComboSDK.GetVariant(),
            serverName = roleInfo.serverName,
            rankScore = UnityEngine.Random.Range(0, 100),
            roundUniqueId = Guid.NewGuid().ToString(),
            roomHostComboId = evt.roomHostId,
            matchType = evt.matchType,
            queuRoleIdList = evt.queueRoleIdList,
            roundResult = evt.roundResult
        };
    }

    [EventSystem.BindEvent]
    void HandleRoundEndEvent(BattleEndEvent evt)
    {
        currentReport = ReportType.BattleResult;
        ChangeTimeView.Instantiate();
        var roleInfo = PlayerController.GetRoleInfo(PlayerController.GetPlayer());
        var gold = GameManager.Instance.gold;
        battleResultReport = new BattleResultReportEvent
        {
            type = "battle_result",
            eventName = "battle_end",
            accountId = ComboSDK.GetLoginInfo().comboId,
            comboId = ComboSDK.GetLoginInfo().comboId,
            serverId = roleInfo.serverId,
            roleId = roleInfo.roleId,
            roleName = roleInfo.roleName,
            distro = ComboSDK.GetDistro(),
            variant = ComboSDK.GetVariant(),
            allianceId = "12345",
            diamond = 0,
            gold = gold,
            pay = (double)gold / 100,
            currentStageId = evt.stageId.ToString(),
            uniqueRequestId = Guid.NewGuid().ToString(),
            stageId = evt.stageId,
            stageType = evt.stageType,
            battleResult = evt.battleType
        };
    }

    [EventSystem.BindEvent]
    void HandleCardDraw(CardDrawEvent evt)
    {
        currentReport = ReportType.CardDraw;
        ChangeTimeView.Instantiate();
        var roleInfo = PlayerController.GetRoleInfo(PlayerController.GetPlayer());
        cardDrawReport = new CardDrawReportEvent
        {
            roleName = roleInfo.roleName,
            accountId = ComboSDK.GetLoginInfo().comboId,
            os = SystemInfo.operatingSystem,
            distro = ComboSDK.GetDistro(),
            variant = ComboSDK.GetVariant(),
            serverName = roleInfo.serverName,
            eventName = "card_draw",
            comboId = ComboSDK.GetLoginInfo().comboId,
            serverId = roleInfo.serverId,
            roleId = roleInfo.roleId,
            heroCnt = evt.heroCnt
        };
    }

    [EventSystem.BindEvent]
    public void ChangeTime(ChangeTimeEvent evt)
    {
        time = ConvertUnixTimeToIso8601(evt.unixTime);
        switch (currentReport)
        {
            case ReportType.Login:
                loginReport.time = time;
                GameClient.ReportEvent(loginReport, (error) => { });
                break;
            case ReportType.ActiveValue:
                activeValueReport.time = time;
                GameClient.ReportEvent(activeValueReport, (error) => { });
                break;
            case ReportType.RoundEnd:
                roundEndReport.time = time;
                GameClient.ReportEvent(roundEndReport, (error) => { });
                break;
            case ReportType.BattleResult:
                battleResultReport.time = time;
                GameClient.ReportEvent(battleResultReport, (error) => { });
                break;
            case ReportType.CardDraw:
                cardDrawReport.time = time;
                GameClient.ReportEvent(cardDrawReport, (error) => { });
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
        // 将 Unix 时间戳转换为 DateTimeOffset
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTime);

        // 定义一个时区信息变量
        TimeZoneInfo chinaTimeZone = TimeZoneInfo.Local;

        // 转换为目标时区的时间
        DateTimeOffset chinaTime = TimeZoneInfo.ConvertTime(dateTimeOffset, chinaTimeZone);

        // 格式化为 ISO 8601 字符串
        return chinaTime.ToString("yyyy-MM-ddTHH:mm:sszzz");
    }
}
