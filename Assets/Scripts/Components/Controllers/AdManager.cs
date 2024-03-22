using System.Collections;
using System.Collections.Generic;
using Combo;
using UnityEngine;

public class AdManager : MonoBehaviour
{
    public PlayerPanel playerPanel;
    private int adReward = 500;
    private bool pendedReward = false;
    void Start()
    {
        EventSystem.Register(this);
    }

    void OnDestroy()
    {
        EventSystem.UnRegister(this);
    }

    [EventSystem.BindEvent]
    void HandlePreloadAdEvent(PreloadAdEvent evt)
    {
        OnPreloadAd(evt.placementId);
    }

    [EventSystem.BindEvent]
    void HandleShowAdEvent(ShowAdEvent evt)
    {
        OnShowAd(evt.placementId);
    }

    public void OnPreloadAd(string placementId)
    {
        if (string.IsNullOrEmpty(placementId))
        {
            Toast.Show($"广告位ID不能为空");
            return;
        }
        UIController.ShowLoading();
        var opts = new PreloadAdOptions
        {
            placementId = placementId,
        };
        Log.I("PreloadAd PlacementId: " + opts.placementId);
        ComboSDK.PreloadAd(opts, r =>
        {
            UIController.HideLoading();
            if (r.IsSuccess)
            {
                var result = r.Data;
                Toast.Show($"广告 {result.placementId} 预加载成功");
                Log.I("广告预加载成功: PlacementId - " + result.placementId);
            }
            else
            {
                var err = r.Error;
                Toast.Show($"广告 {opts.placementId} 预加载失败\n{err.Message}");
                Log.E("广告预加载失败: " + err.DetailMessage);
            }
        });
    }

    public void OnShowAd(string placementId)
    {
        if (string.IsNullOrEmpty(placementId))
        {
            Toast.Show($"广告位ID不能为空");
            return;
        }
        var opts = new ShowAdOptions
        {
            placementId = placementId,
        };
        UIController.ShowLoading();
        ComboSDK.ShowAd(opts, r =>
        {
            UIController.HideLoading();
            if (r.IsSuccess)
            {
                var result = r.Data;
                if(result.status == ShowAdStatus.START)
                {
                    Toast.Show($"广告 {result.token} 开始播放");
                    Log.I($"广告开始播放: Status - {result.status}; Token - {result.token}");
                }
                else if(result.status == ShowAdStatus.CLICKED)
                {
                    Toast.Show($"广告 {result.token} 被点击");
                    Log.I($"广告被点击: Status - {result.status}; Token - {result.token}");
                }
                else if(result.status == ShowAdStatus.REWARDED)
                {
                    pendedReward = true;
                    RequestUpdateCoinEvent.Invoke(new RequestUpdateCoinEvent { coinOffset = adReward });
                    Toast.Show($"广告 {result.token} 播放完成，获得奖励");
                    Log.I($"广告播放完成，获得奖励: Status - {result.status}; Token - {result.token}");
                }
                else{
                    Toast.Show($"广告 {result.token} 被关闭");
                    Log.I($"广告被关闭: Status - {result.status}; Token - {result.token}");
                }
            }
            else
            {
                var err = r.Error;
                Toast.Show($"广告 {opts.placementId} 显示失败\n{err.Message}");
                Log.E("广告显示失败: " + err.DetailMessage);
            }
        });
    }

    [EventSystem.BindEvent]
    private void OnCoinUpdated(CoinUpdatedEvent evt) {
        if (!pendedReward) {
            return;
        }

        pendedReward = false;
        UIController.Alert(UIAlertType.Stackable, "收到奖励", $"获得 {adReward} 金币，当前有 {evt.coin} 金币", "确定", ()=>{});
    }
}
