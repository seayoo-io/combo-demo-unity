using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ComboSDKConfig
{
    public List<string> domains;

    // 是否支分享
    public bool supportShare  { get; private set; }
    // 是否支持广告
    public bool supportAds { get; private set; }
    private ComboSDKConfig() { }
    public ComboSDKConfig(List<string> domains)
    {
        if (domains == null || domains.Count == 0)
        {
            throw new ArgumentException("ComboSDK 配置数据为空或无效");
        }
        try
        {
            this.domains = domains;
            InitializeProperties();
        }
        catch (Exception e)
        {
            Debug.LogError($"解析 ComboSDK 配置数据失败: {e.Message}");
            throw; 
        }
    }

    // 初始化属性
    private void InitializeProperties()
    {
        // 初始化 supportShare
        List<string> shareDomains = new List<string>
        {
            "agora",
            "douyin_open",
            "qq",
            "weibo",
            "weixin"
        };
        supportShare = domains.Exists(domain => shareDomains.Contains(domain));
        // 初始化 supportAds
        List<string> adsDomains = new List<string>
        {
            "topon",
            "honor_ads",
            "oppo_ads",
            "xiaomi_ads",
            "vivo_ads",
            "huawei_ads",
            "4399_ads"
        };
        supportAds = domains.Exists(domain => adsDomains.Contains(domain));
    }
}