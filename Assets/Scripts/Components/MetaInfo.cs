using System.Collections;
using System.Collections.Generic;
using Combo;
using UnityEngine;
using UnityEngine.UI;

public class MetaInfo : MonoBehaviour
{
    public Text metaText;
    private static MetaInfo _instance;

    void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {  
        var paramz = BuildParams.Load();
        metaText.text =
            $@"DeviceId:{ComboSDK.GetDeviceId()} - Branch:{paramz.branchName} - BuildNum:{paramz.buildNumber}
ComboSDK v{ComboSDK.GetVersion()} with {GameUtils.GetPlatformName()}SDK v{ComboSDK.GetVersionNative()} GameId:{ComboSDK.GetGameId()} - Distro:{ComboSDK.GetDistro()}";
#if UNITY_ANDROID
        metaText.text += $" - Variant:{ComboSDK.GetVariant()} - Subvariant:{ComboSDK.GetSubvariant()}";
#endif
    }
}
