using UnityEngine;
using System.Collections;
#if !UNITY_STANDALONE_WIN
using com.xsj.Crasheye.U3D;
#endif
using System.Runtime.InteropServices;
using System;

public class CrasheyeForIOS : MonoBehaviour
{
#if UNITY_IOS && !UNITY_STANDALONE_WIN

    [DllImport("__Internal")]
	private static extern void crasheyeInit(string appkey);
	
	[DllImport("__Internal")]
	private static extern void crasheyeInitWithChannel(string appkey, string channel);
	
	[DllImport("__Internal")]
	private static extern void crasheyeAddExtraData(string key, string value);
	
	[DllImport("__Internal")]
	private static extern void crasheyeRemoveExtraData(string key);
	
	[DllImport("__Internal")]
	private static extern void crasheyeClearExtraData();
	
	[DllImport("__Internal")]
	private static extern void crasheyeLeaveBreadcrumb(string breadcrumb);
	
	[DllImport("__Internal")]
	private static extern void crasheyeSetUserid(string userid);

	[DllImport("__Internal")]
	private static extern void crasheyeSetAppVersion(string version);

	[DllImport("__Internal")]
	private static extern void crasheyeSetFlushOnlyOverWiFi (int enabled);
	[DllImport("__Internal")]
	private static extern void crasheyeAddLog(string log);

    [DllImport("__Internal")]
	private static extern void crasheyeSetURL(string url);

	[DllImport("__Internal")]
	private static extern void crasheyeRemoveLog();

	[DllImport("__Internal")]
	private static extern void crasheyeSetBeta (int enabled);

    /// <summary>
    /// 最大保存log日志数量
    /// </summary>
    private static int MAX_LOG_COUNT = 300;

    private static Queue m_saveLog = new Queue();
                
    /// <summary>
    /// iOS系统的初始
    /// </summary>
    /// <param name="appKeyForIOS"></param>
    /// <param name="channIdForIOS"></param>
    public static void Init(string appKeyForIOS, string channIdForIOS)
    {
        if (string.IsNullOrEmpty(appKeyForIOS))
        {
            Debug.LogError("app key is null or empty!");
            return;
        }

        crasheyeInitWithChannel(appKeyForIOS, channIdForIOS);

        Crasheye.crasheyeLib.InitCrasheyeLib();        
        Crasheye.crasheyeLib.SetExceptionCallback();
    }
    
    /// <summary>
    /// 设置是否打印调试日志
    /// </summary>
    /// <param name="debugLog">默认为False为不打印日志；True为打印日志</param>
    public static void SetDebugLog(bool debugLog)
    {
        // 暂时未实现
    }

    /// <summary>
    /// 设置版本号信息
    /// </summary>
    /// <param name="appVersion"></param>
    public static void SetAppVersion(string appVersion)
    {
		if (string.IsNullOrEmpty(appVersion))
		{
			Debug.LogError("appVersion is null or empty!");
			return;
		}

		try
		{
			crasheyeSetAppVersion(appVersion);
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message);
		}
    }

    public static void SetUserIdentifier(string userIdentifier)
    {
        if (string.IsNullOrEmpty(userIdentifier))
        {
            Debug.LogError("user identifier is null or empty!");
            return;
        }

        try
        {
            crasheyeSetUserid(userIdentifier);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    private static int m_lines = -1;

    /// <summary> 指定获取应用程序log日志的行数
    /// </summary>
    /// <param name="lines">设置获取行号</param>
    public static void SetLogging(int lines)
    {
        if (lines < 0)
        {
            return;
        }

        if (lines > MAX_LOG_COUNT)
        {
            m_lines = MAX_LOG_COUNT;
        }
        else
        {
            m_lines = lines;
        }
    }

    /// <summary>
    /// 返回设置log行数
    /// </summary>
    /// <returns></returns>
    public static int GetLoggingLines()
    {
        return m_lines;
    }

    private static string m_filter = "";
    /// <summary> 获取应用程序log日志关键字过滤
    /// </summary>
    /// <param name="filter">设置获取关键字</param>
    public static void SetLogging(string filter)
    {
        if (string.IsNullOrEmpty(filter))
        {
            return;
        }

        m_filter = filter;
    }

    /// <summary>
    /// 返回log过虑
    /// </summary>
    /// <returns></returns>
    public static string GetLoggingFilter()
    {
        return m_filter;
    }

    /// <summary> 获取应用程序log日志（过滤条件：关键字过滤+行数）
    /// </summary>
    /// <param name="lines">设置获取行数</param>
    /// <param name="filter">设置获取关键字</param>
    public static void SetLogging(int lines, string filter)
    {
        if (lines < 0 || string.IsNullOrEmpty(filter))
        {
            return;
        }
        SetLogging(lines);
        SetLogging(filter);
    }

    public static void SetFlushOnlyOverWiFi(bool enabled)
    {
		if(enabled)
			crasheyeSetFlushOnlyOverWiFi (1);
		else
			crasheyeSetFlushOnlyOverWiFi (0);
    }

	public static void SetBeta(bool enabled)
	{
		if(enabled)
			crasheyeSetBeta (1);
		else
			crasheyeSetBeta (0);
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
            Debug.LogError("add extra data err: key or value IsNullOrEmpty!");
            return;
        }
        crasheyeAddExtraData(key, value);
    }

    /// <summary>
    /// 获取自定义数据
    /// </summary>
    /// <param name="key">Key</param>
    public static string GetExtraData(string key)
    {
        // 暂时未实现
        return "";
    }

    /// <summary>
    /// 移除自定义数据
    /// </summary>
    /// <param name="key">Key.</param>
    public static void RemoveExtraData(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        crasheyeRemoveExtraData(key);
    }

    /// <summary>
    /// 添除数据
    /// </summary>
    public static void ClearExtraData()
    {
        crasheyeClearExtraData();
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

        crasheyeLeaveBreadcrumb(breadcrumb);
    }

    /// <summary> 获取SDK版本信息
    /// </summary>
    /// <returns>返回sdk版本号</returns>
    public static string GetAppVersion()
    {
        // 此功能暂未实现
        string sdkVersion = "NA";
        return sdkVersion;
    }
    
    public static void SaveLogger(string logMsg)
    {
        if (CrasheyeForIOS.GetLoggingLines() <= 0)
        {
            return;
        }

        if (logMsg.IndexOf(CrasheyeForIOS.GetLoggingFilter()) < 0)
        {
            return;
        }

        if (m_saveLog.Count >= CrasheyeForIOS.GetLoggingLines() && m_saveLog.Count > 0)
        {
            m_saveLog.Dequeue();
        }

        m_saveLog.Enqueue(logMsg);
    }

    public static string GetLogger()
    {
        string allLogger = "";
        foreach (string strTemp in m_saveLog)
        {
			allLogger += strTemp + "\n";
        }
        return allLogger;
    }

	public static void addLog(string log)
	{
		crasheyeAddLog(log);
	}

	public static void removeLog()
	{
		crasheyeRemoveLog();
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

        crasheyeSetURL(url);

    }

#endif
}
