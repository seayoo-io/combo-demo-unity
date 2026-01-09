using UnityEngine;
using System.Collections;
#if !UNITY_STANDALONE_WIN
using com.xsj.Crasheye.U3D;
#endif
using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Runtime.CompilerServices;

public class CrasheyeForAndroid : MonoBehaviour
{
#if UNITY_ANDROID && !UNITY_STANDALONE_WIN

    /// <summary> 通过JNI调用Java添加自定义数据
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    [DllImport("CrasheyeNDK")]
    private static extern void SetCustomData(string key, string value);

    /// <summary> 通过JNI调用
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    [DllImport("CrasheyeNDK")]
    private static extern string GetCustomData(string key);

    /// <summary> 通过JNI移除自定义数据
    /// </summary>
    /// <param name="pKey"></param>
    [DllImport("CrasheyeNDK")]
    private static extern void RemoveCustomData(string pKey);

    /// <summary> 通过JNI清除自定义数据
    /// </summary>
    [DllImport("CrasheyeNDK")]
    private static extern void CleanCustomData();

    /// <summary> 通过JNI调用Java添加打点信息
    /// </summary>
    /// <param name="breadcrumb"></param>
    [DllImport("CrasheyeNDK")]
    private static extern void LeaveBreadcrumbData(string breadcrumb);
            
    /// <summary>
    /// 崩溃回溯C#堆栈信息
    /// </summary>
    private static bool CSharpStackTrace = false;

    /// <summary>
    /// 用于调用java函数
    /// </summary>
    private static AndroidJavaClass dumpcls;

    /// <summary>
    /// 设置AppVersion信息
    /// </summary>
    private static string YourAppVersion = "NA";

    /// <summary>
    /// 设置渠道号
    /// </summary>
    private static string YourChannelId = "NA";

    /// <summary>
    /// 设置是否只在wifi下往服务器发送数据
    /// </summary>
    private static bool FlushOnlyOverWiFi = false;
    
    /// <summary>
    /// Android的初始化
    /// </summary>
    /// <param name="appKeyForAndroid"></param>
    /// <param name="channIdForAndroid"></param>
    public static void Init(string appKeyForAndroid, string channIdForAndroid)
    {
        if (string.IsNullOrEmpty(appKeyForAndroid))
        {
            return;
        }

        AndroidJavaClass cls = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject javaObj = cls.GetStatic<AndroidJavaObject>("currentActivity");

        EnableDebug();

        dumpcls = new AndroidJavaClass("com.xsj.crasheye.Crasheye");

        if (!YourAppVersion.Equals("NA"))
        {
            dumpcls.CallStatic("setAppVersion", new object[] { YourAppVersion });
        }

        if (FlushOnlyOverWiFi)
        {   // 默认什为false
            dumpcls.CallStatic("setFlushOnlyOverWiFi", new object[] { FlushOnlyOverWiFi });
        }

        dumpcls.CallStatic("setChannelID", new object[] { channIdForAndroid });
        dumpcls.CallStatic("initWithNativeHandleUserspaceSig", new object[] { javaObj, appKeyForAndroid });

        if (CSharpStackTrace)
        {
            Crasheye.crasheyeLib.InitCrasheyeLib();
        }        
        Crasheye.crasheyeLib.SetExceptionCallback();
    }

    /// <summary>
    /// 如果启动了C#堆栈回溯可能会导致某些机型出现宕机
    /// </summary>
    public static void EnableCSharpStackTrace()
    {
        CSharpStackTrace = true;
    }

    /// <summary> 设置版本号信息
    /// </summary>
    /// <param name="yourAppVersion"></param>
    public static void SetAppVersion(string yourAppVersion)
    {
        if (yourAppVersion == null)
        {
            return;
        }
        YourAppVersion = yourAppVersion;
    }

    /// <summary> 获取SDK版本信息
    /// </summary>
    /// <returns>返回sdk版本号</returns>
    public static string GetAppVersion()
    {
        try
        {
            dumpcls = new AndroidJavaClass("com.xsj.crasheye.Crasheye");
            if (dumpcls == null)
            {
                return "NA";
            }

            string sdkVersion = dumpcls.CallStatic<string>("getSDKVersion", new object[] { });
            if (string.IsNullOrEmpty(sdkVersion))
            {
                return "NA";
            }

            return sdkVersion;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            return "NA";
        }
    }

    /// <summary> 设置用户信息
    /// </summary>
    /// <param name="userIdentifier"></param>
    public static void SetUserIdentifier(string userIdentifier)
    {
        if (String.IsNullOrEmpty(userIdentifier))
        {
            Debug.LogError("set user identifier is null or empty!");
            return;
        }

        try
        {
            dumpcls = new AndroidJavaClass("com.xsj.crasheye.Crasheye");
            if (dumpcls == null)
            {
                return;
            }

            dumpcls.CallStatic("setUserIdentifier", new object[] { userIdentifier });
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    /// <summary> SetURL
    /// </summary>
    /// <param name="SetURL"></param>
    public static void SetURL(string url)
    {
        if (String.IsNullOrEmpty(url))
        {
            Debug.LogError("set url is null or empty!");
            return;
        }

        try
        {
            dumpcls = new AndroidJavaClass("com.xsj.crasheye.Crasheye");
            if (dumpcls == null)
            {
                return;
            }

            dumpcls.CallStatic("setURL", new object[] { url });
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    /// <summary> 指定获取应用程序log日志的行数
    /// </summary>
    /// <param name="lines">设置获取行号</param>
    public static void SetLogging(int lines)
    {
        try
        {
            dumpcls = new AndroidJavaClass("com.xsj.crasheye.Crasheye");
            if (dumpcls == null)
            {
                return;
            }

            dumpcls.CallStatic("setLogging", new object[] { lines });
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    /// <summary> 获取应用程序log日志关键字过滤
    /// </summary>
    /// <param name="filter">设置获取关键字</param>
    public static void SetLogging(string filter)
    {
        if (string.IsNullOrEmpty(filter))
        {
            return;
        }

        try
        {
            dumpcls = new AndroidJavaClass("com.xsj.crasheye.Crasheye");
            if (dumpcls == null)
            {
                return;
            }

            dumpcls.CallStatic("setLogging", new object[] { filter });
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    /// <summary> 获取应用程序log日志（过滤条件：关键字过滤+行数）
    /// </summary>
    /// <param name="lines">设置获取行数</param>
    /// <param name="filter">设置获取关键字</param>
    public static void SetLogging(int lines, string filter)
    {
        if (string.IsNullOrEmpty(filter))
        {
            return;
        }

        try
        {
            dumpcls = new AndroidJavaClass("com.xsj.crasheye.Crasheye");
            if (dumpcls == null)
            {
                return;
            }

            dumpcls.CallStatic("setLogging", new object[] { lines, filter });
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public static void SetIsBetaVersion(bool isBeta)
    {
       try
        {
            dumpcls = new AndroidJavaClass("com.xsj.crasheye.Crasheye");
            if (dumpcls == null)
            {
                return;
            }

            dumpcls.CallStatic("setIsBetaVersion", new object[] { isBeta });
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        } 
    }

    /// <summary> 设置是否在Wifi下往服务器发送信息
    /// </summary>
    /// <param name="enabled"></param>
    public static void SetFlushOnlyOverWiFi(bool enabled)
    {
        FlushOnlyOverWiFi = enabled;
    }

    /// <summary>
    /// 打调试信息设置打开
    /// </summary>
    public static void EnableDebug()
    {
        try
        {
            dumpcls = new AndroidJavaClass("com.xsj.crasheye.Crasheye");
            dumpcls.CallStatic("enableDebug", new object[]{});
        }
        catch (Exception ex)
        {
            Debug.LogError("call java set debug log err:" + ex.Message);
        }
    }
    
    /// <summary>
    /// 添加自定义数据
    /// </summary>
    /// <param name="key">Key.</param>
    /// <param name="value">Value.</param>
    public static void AddExtraData(string key, string value)
    {
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
        {
            return;
        }

        SetCustomData(key, value);

        try
        {
            dumpcls = new AndroidJavaClass("com.xsj.crasheye.Crasheye");
            if (dumpcls == null)
            {
                return;
            }

            dumpcls.CallStatic("addExtraData", new object[] { key, value });
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    /// <summary>
    /// 获取自定义的值
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string GetExtraData(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return "NA";
        }
        return GetCustomData(key);
    }

    /// <summary>
    /// 移除自定义数据
    /// </summary>
    /// <param name="key"></param>
    public static void RemoveExtraData(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }
        RemoveCustomData(key);
    }

    /// <summary>
    /// 清空所有自定数据
    /// </summary>
    public static void CleanExtraData()
    {
        CleanCustomData();
    }

    /// <summary>
    /// 打点数据
    /// </summary>
    /// <param name="breadcrumb">Breadcrumb.</param>
    public static void LeaveBreadcrumb(string breadcrumb)
    {
        if (string.IsNullOrEmpty(breadcrumb))
        {
            return;
        }
        LeaveBreadcrumbData(breadcrumb);

        try
        {
            dumpcls = new AndroidJavaClass("com.xsj.crasheye.Crasheye");
            if (dumpcls == null)
            {
                return;
            }

            dumpcls.CallStatic("leaveBreadcrumb", new object[] { breadcrumb });
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public static void setInternalExtraData()
    {
        var internalExtraData = GetHardwareInfo();
        if (String.IsNullOrEmpty(internalExtraData))
        {
            Debug.LogError("set internalExtraData is null or empty!");
            return;
        }

        try
        {
            dumpcls = new AndroidJavaClass("com.xsj.crasheye.Crasheye");
            if (dumpcls == null)
            {
                return;
            }

            dumpcls.CallStatic("setInternalExtraData", new object[] { internalExtraData });
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    private static string GetCpuInfo()
    {
        string cpuInfo = "NA";
        TextReader file = null;
        AndroidJavaClass androidOSBuild = new AndroidJavaClass("android.os.Build");

        if (File.Exists("/proc/cpuinfo"))
        {
            file = File.OpenText("/proc/cpuinfo");
        }
        
        if (file == null)
        {
            cpuInfo = androidOSBuild.GetStatic<string>("HARDWARE");
            return cpuInfo;
        }

        try
        {
            string line;
            while ((line = file.ReadLine()) != null)
            {
                if (line.ToLower().IndexOf("hardware") != -1)
                {
                    string[] partArr = line.Split(':');
                    if (partArr.Length > 0)
                    {
                        cpuInfo = partArr[partArr.Length - 1].Trim();
                        break;
                    }
                }
            }
            if (cpuInfo == "NA")
            {
                cpuInfo = androidOSBuild.GetStatic<string>("HARDWARE");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }

        return cpuInfo;
    }

    private static string GetHardwareInfo()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(SystemInfo.graphicsDeviceName);
        sb.Append("###" + GetCpuInfo());

        return sb.ToString();
    }
#endif

}
