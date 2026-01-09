/**
 *  Unity Plugins Version   2.2.10
 *  
 *      android version     2.2.9
 *      iOS version         2.7.3
 *      window version      1.0.0
 */
using System.Text;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
#if UNITY_ANDROID || UNITY_IPHONE || UNITY_IOS
using com.xsj.Crasheye.U3D;
#endif

#if UNITY_ANDROID
using com.xsj.Crasheye.U3D.Android;
#endif


#if UNITY_IPHONE || UNITY_IOS
using com.xsj.Crasheye.U3D.IOS;
#endif
public class Crasheye : MonoBehaviour
{
    public delegate void FnOnCrashCallback(bool bCaptureSucceed, string cpszCrashReportFile);
    
    public string YourAppKeyForAndroid = "YourAppKeyForAndroid";
    public string YourChannelIdForAndroid = "YourChannelIdForAndroid";

    public string YourAppKeyForIOS = "YourAppKeyForIOS";
    public string YourChannelIdForIOS = "YourChannelIdForIOS";

    public string YourAppKeyForPC = "YourAppKeyForPC";
    public string YourChannelIdForPC = "YourChannelIdForPC";

#if UNITY_ANDROID || UNITY_IPHONE || UNITY_IOS
    public static CrasheyeLib crasheyeLib = null;
#endif
    
#if UNITY_ANDROID || UNITY_IPHONE || UNITY_IOS || UNITY_STANDALONE_WIN
    private static string YourChannelId = "NA";
#endif
    public static void AddCustomLog(string collectFile)
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        DumpForPC.AddCustomLog(collectFile);
#endif
    }


    public static void AddCustomLogDirectory(string CollectLogDirectory)
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        DumpForPC.AddCustomLogDirectory(CollectLogDirectory);
#endif
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);

        if (Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
            SetChannelID(YourChannelIdForAndroid);
            StartInitCrasheye(YourAppKeyForAndroid);
#endif
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IPHONE || UNITY_IOS
            SetChannelID(YourChannelIdForIOS);
            StartInitCrasheye(YourAppKeyForIOS);
#endif
        }
        else if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
#if UNITY_STANDALONE_WIN
            SetChannelID(YourAppKeyForPC);
            StartInitCrasheye(YourAppKeyForPC);
#endif
        }
    }

    /// <summary>
    /// SetURL
    /// </summary>
    /// <param name="SetUrl"></param>
    public static void SetURL(string url)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
            CrasheyeForAndroid.SetURL(url);
#endif
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IPHONE || UNITY_IOS
            CrasheyeForIOS.SetURL(url);
#endif
        }
        else if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
#if UNITY_STANDALONE_WIN
            DumpForPC.SetURL(url);
#endif
        }
    }
    
    /// <summary>
    ///  设置Dump是否强制上报（忽略玩家在DumpReport界面上的选择）
    ///
    /// </summary>
    /// <param name="isForceUpload"> isForceUpload   是否设置Dump强制上报 </param>
    public static void SetForceUpload(bool isForceUpload)
    {
        Debug.Log("SetForceUpload");
#if UNITY_STANDALONE_WIN
        DumpForPC.SetForceUpload(isForceUpload);
#endif
    }
    /// <summary>
    /// 启动Crasheye
    /// </summary>
    /// <param name="Your_AppKey"></param>
    public static void StartInitCrasheye(string Your_AppKey)
    {        
        RegisterLogCallback();
        
        if (Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
            CrasheyeForAndroid.Init(Your_AppKey, YourChannelId);
#endif
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IPHONE || UNITY_IOS
            CrasheyeForIOS.Init(Your_AppKey, YourChannelId);
#endif
        }
        else if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
#if UNITY_STANDALONE_WIN
            DumpForPC.Init(Your_AppKey, YourChannelId);
#endif
        }

        //bool result = Crasheye.PushLogTrace("Crasheye Demo Push Log");
        //Debug.Log("PushLogTrace:" + result);
    }

    /// <summary>
    /// 注册脚本捕获
    /// </summary>
    public static void RegisterLogCallback()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
            crasheyeLib = new LibForAndroid();

            CrasheyeForAndroid.setInternalExtraData();
#endif
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IPHONE || UNITY_IOS
            crasheyeLib = new LibForiOS();
#endif
        }
        else if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
#if UNITY_STANDALONE_WIN
            Debug.Log("RegisterLogCallback");
            AppDomain.CurrentDomain.UnhandledException += DumpForPC.OnHandleUnresolvedException;
            SetRegisterLogFunction(DumpForPC.OnHandleLogCallback);
            Application.logMessageReceived += RegisterLogFunction;
#endif
        }

#if UNITY_ANDROID || UNITY_IPHONE || UNITY_IOS
        if (crasheyeLib == null)
        {
            return;
        }

        System.AppDomain.CurrentDomain.UnhandledException += new System.UnhandledExceptionEventHandler(crasheyeLib.OnHandleUnresolvedException);

        SetRegisterLogFunction(crasheyeLib.OnHandleLogCallback);

#if UNITY_5
                Application.logMessageReceived += RegisterLogFunction;
#else
                Application.RegisterLogCallback(RegisterLogFunction);
#endif
#endif
    }

    private static void RegisterLogFunction(string logString, string stackTrace, LogType type)
    {
#if (UNITY_IPHONE || UNITY_IOS) && !UNITY_EDITOR
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			CrasheyeForIOS.SaveLogger(logString);
		}

		if (CrasheyeForIOS.GetLoggingLines() > 0 && 
            Application.platform == RuntimePlatform.IPhonePlayer && 
            (type == LogType.Assert || type == LogType.Exception)
           )
		{
			CrasheyeForIOS.addLog(CrasheyeForIOS.GetLogger());
		}
#endif
        if (m_RegisterLog != null)
        {
            m_RegisterLog(logString, stackTrace, type);
        }

#if (UNITY_IPHONE || UNITY_IOS) && !UNITY_EDITOR
		if (CrasheyeForIOS.GetLoggingLines() > 0 && 
            Application.platform == RuntimePlatform.IPhonePlayer && 
            (type == LogType.Assert || type == LogType.Exception)
            )
		{

			CrasheyeForIOS.removeLog();
		}
#endif
    }

    public delegate void RegisterLog(string logString, string stackTrace, LogType type);
    private static RegisterLog m_RegisterLog = null;
   
    /// <summary>
    /// 设置用户的脚本回调的函数
    /// </summary>
    /// <param name="registerLogCBFun"></param>
    public static void SetRegisterLogFunction(RegisterLog registerLogCBFun)
    {
        Debug.Log("SetRegisterLogFunction");
        m_RegisterLog += registerLogCBFun;
    }
    
    /// <summary>
    /// 发送脚本异常
    /// </summary>
    /// <param name="ex">Excepiton Info</param>
    public static void SendScriptException(Exception ex)
    {        
        if (Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
            crasheyeLib.LibSendScriptException(ex.Message, ex.StackTrace);
#endif
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IPHONE || UNITY_IOS
            crasheyeLib.LibSendScriptException(ex.Message, ex.StackTrace);
#endif
        }
    }

    /// <summary>
    /// 上报脚本异常
    /// </summary>
    /// <param name="errorTitle">错误的标题</param>
    /// <param name="stacktrace">错误堆栈信息</param>
    /// <param name="language">语言</param>
    public static void SendScriptException(string errorTitle, string stacktrace, string language)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
            crasheyeLib.LibSendScriptException(errorTitle, stacktrace, language);
#endif
        }
        else if(Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IPHONE || UNITY_IOS
            crasheyeLib.LibSendScriptException(errorTitle, stacktrace);
#endif
        } else if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
#if UNITY_STANDALONE_WIN
            DumpForPC.SendScriptException(errorTitle, stacktrace, language);
#endif
        }
    }
    
    /// <summary>
    /// 设置渠道号
    /// </summary>
    /// <param name="yourChannelID"></param>
    public static void SetChannelID(string yourChannelID)
    {
        if (String.IsNullOrEmpty(yourChannelID))
        {
            Debug.LogError("set channel id value is null or empty!");
            return;
        }

        if (yourChannelID.Equals("YourChannelIdForAndroid") || yourChannelID.Equals("YourChannelIdForIOS"))
        {
            return;
        }

#if UNITY_ANDROID || UNITY_IPHONE || UNITY_IOS || UNITY_STANDALONE_WIN
        YourChannelId = yourChannelID;
#endif
    }

    /// <summary>
    /// 设置是否只在wifi下上报报告文件
    /// </summary>
    /// <param name="enabled"></param>
    public static void SetFlushOnlyOverWiFi(bool enabled)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
            CrasheyeForAndroid.SetFlushOnlyOverWiFi(enabled);
#endif
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IPHONE || UNITY_IOS
            CrasheyeForIOS.SetFlushOnlyOverWiFi(enabled);
#endif
        }
    }

    /// <summary>
    /// 设置该版本是否为测试版本
    /// </summary>
    /// <param name="isBeta">是否为测试版本</param>
    public static void SetIsBetaVersion(bool isBeta)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
            CrasheyeForAndroid.SetIsBetaVersion(isBeta);
#endif
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IPHONE || UNITY_IOS
            CrasheyeForIOS.SetBeta(isBeta);
#endif
        }
        else if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            DumpForPC.SetBeta();
#endif
        }
    }


    /// <summary>
    /// 设置版本号信息
    /// </summary>
    /// <param name="yourAppVersion">App版本号</param>
    public static void SetAppVersion(string yourAppVersion)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
            CrasheyeForAndroid.SetAppVersion(yourAppVersion);
#endif
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IPHONE || UNITY_IOS
            CrasheyeForIOS.SetAppVersion(yourAppVersion);
#endif
        }
        else if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
#if UNITY_STANDALONE_WIN
            DumpForPC.SetAppVersion(yourAppVersion);
#endif
        }
    }

    /// <summary>
    /// 设置用户信息
    /// </summary>
    /// <param name="setUserIdentifier">用户标识</param>
    public static void SetUserIdentifier(string userIdentifier)
    {
        if(string.IsNullOrEmpty(userIdentifier))
        {
            return;
        }

        if (Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
            CrasheyeForAndroid.SetUserIdentifier(userIdentifier);
#endif
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IPHONE || UNITY_IOS
            CrasheyeForIOS.SetUserIdentifier(userIdentifier);
#endif
        }
        else if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
#if UNITY_STANDALONE_WIN
            DumpForPC.SetUserIdentifier_UTF8(userIdentifier);
#endif
        }
    }

    /// <summary>
    /// 如果启动了C#堆栈回溯可能会导致某些机型出现宕机
    /// </summary>
    public static void EnableCSharpStackTrace()
    {
#if UNITY_ANDROID
        CrasheyeForAndroid.EnableCSharpStackTrace();
#endif
    }

    /// <summary>
    /// 添加自定义数据
    /// </summary>
    /// <param name="key">Key.</param>
    /// <param name="value">Value.</param>
    public static void AddExtraData(string key, string value)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
            CrasheyeForAndroid.AddExtraData(key, value);
#endif
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IPHONE || UNITY_IOS
            CrasheyeForIOS.AddExtraData(key, value);
#endif
        }
        else if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
#if UNITY_STANDALONE_WIN
            DumpForPC.AddExtraData(key,value);
#endif
        }
    }

    public static void AddExtraDataUTF8(string key, string value)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
            CrasheyeForAndroid.AddExtraData(key, value);
#endif
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IPHONE || UNITY_IOS
            CrasheyeForIOS.AddExtraData(key, value);
#endif
        }
        else if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
#if UNITY_STANDALONE_WIN
            DumpForPC.AddExtraDataUTF8(key, value);
#endif
        }
    }

    /// <summary>
    /// 获取自定义数据
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns></returns>
    public static string GetExtraData(string key)
    {
        string extraData = "";

        if (string.IsNullOrEmpty(key))
        {
            return extraData;
        }

        if (Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
            extraData = CrasheyeForAndroid.GetExtraData(key);
#endif
        }
        else
        {
#if UNITY_IPHONE || UNITY_IOS
            extraData = CrasheyeForIOS.GetExtraData(key);
#endif
        }

        return extraData;
    }

    /// <summary>
    /// 移除自定义值
    /// </summary>
    /// <param name="key">Key</param>
    public static void RemoveExtraData(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        if (Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
            CrasheyeForAndroid.RemoveExtraData(key);
#endif
        }
        else if(Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IPHONE || UNITY_IOS
            CrasheyeForIOS.RemoveExtraData(key);
#endif
        }
    }

    /// <summary>
    /// 清除自定义数据
    /// </summary>
    public static void CleanExtraData()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
            CrasheyeForAndroid.CleanExtraData();
#endif
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IPHONE || UNITY_IOS
            CrasheyeForIOS.ClearExtraData();
#endif
        }
    }

    /// <summary>
    /// 设置打点数据接口
    /// </summary>
    /// <param name="breadcrumb">Breadcrumb.</param>
    public static void SetBreadCrumbType(Int32 type)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
            // TODO            
#endif
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IPHONE || UNITY_IOS
            // TODO
#endif
        }
        else if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
#if UNITY_STANDALONE_WIN
            DumpForPC.SetBreadCrumbType(type);
#endif
        }
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

        if (Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
            CrasheyeForAndroid.LeaveBreadcrumb(breadcrumb);
#endif
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IPHONE || UNITY_IOS
            CrasheyeForIOS.LeaveBreadcrumb(breadcrumb);
#endif
        } else if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
#if UNITY_STANDALONE_WIN
            DumpForPC.leaveBreadcrumb(breadcrumb);
#endif
        }
    }

    /// <summary>
    /// 指定获取应用程序log日志的行数
    /// </summary>
    /// <param name="lines">需要获取log行数</param>
    public static void SetLogging(int lines)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
            CrasheyeForAndroid.SetLogging(lines);
#endif
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IPHONE || UNITY_IOS
            CrasheyeForIOS.SetLogging(lines);
#endif
        }
    }

    /// <summary>
    /// 获取应用程序log日志关键字过滤
    /// </summary>
    /// <param name="filter">需要过滤关键字</param>
    public static void SetLogging(string filter)
    {
        if (string.IsNullOrEmpty(filter))
        {
            return;
        }

        if (Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
            CrasheyeForAndroid.SetLogging(filter);
#endif
        }
    }

    /// <summary>
    /// 获取应用程序log日志（过滤条件：关键字过滤+行数）
    /// </summary>
    /// <param name="lines">需要获取的行数</param>
    /// <param name="filter">需要过滤的关键字</param>
    public static void SetLogging(int lines, string filter)
    {
        if (string.IsNullOrEmpty(filter))
        {
            return;
        }

        if (Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
            CrasheyeForAndroid.SetLogging(lines, filter);
#endif
        }
        else if(Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IPHONE || UNITY_IOS
            CrasheyeForIOS.SetLogging(lines, filter);
#endif
        }
    }

    /// <summary>
    /// 获取sdk版本号
    /// </summary>
    /// <returns></returns>
    public static string GetAppVersion()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
            return CrasheyeForAndroid.GetAppVersion();
#endif
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IPHONE || UNITY_IOS
            return CrasheyeForIOS.GetAppVersion();
#endif
        }
        return "NA";
    }

    public static bool SetBackgroundUpload(bool isBackgroundUpload)
    {
        
#if UNITY_STANDALONE_WIN
        return DumpForPC.SetBackgroundUpload(isBackgroundUpload);
#endif
        return false;
    }
    
    public static bool SetCrashCallback(FnOnCrashCallback pCallback)
    {

#if UNITY_STANDALONE_WIN
        return DumpForPC.SetOnMiniDumpCreateCallBack((bCaptureSucceed, cpszCrashReportFile)=>{
            string  szCrashReportFile = Marshal.PtrToStringUni(cpszCrashReportFile);
            pCallback?.Invoke(bCaptureSucceed, szCrashReportFile);
        });
#endif
        return false;
    }
    
    public static bool PushLogTrace(string cpszMessage)
    {
#if UNITY_STANDALONE_WIN
        return DumpForPC.PushLogTrace(cpszMessage);
#endif
        return false;
    }
}