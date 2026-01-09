#pragma once
#ifdef _WIN32
#include <Windows.h>

#ifdef _DEBUG		// is debug.
#ifdef _WIN64		// is x64
#define DUMPER_DLL	L".\\CrasheyeD64.dll"
#else				// is x86
#define DUMPER_DLL	L".\\CrasheyeD.dll"
#endif				// _WIN64
#else				// is release.
#ifdef _WIN64		// is x64
#define DUMPER_DLL	L".\\Crasheye64.dll"
#else				// is x86
#define DUMPER_DLL	L".\\Crasheye.dll"
#endif				// _WIN64
#endif				// _DEBUG

namespace Crasheye
{
    typedef void (*FnOnCrashCallback)(BOOL bCaptureSucceed, const char* cpszCrashReportFile);
    struct ApiHolder
    {
        enum DumpCommandType
        {      
			enumDump_Begin = 0,

			enumDump_BackgroundExecutionFlag = enumDump_Begin,		// 后台上传宕机信息（不显示DumpReport界面）
			enumDump_ForceUpload,									// Dump是否强制上报（忽略玩家在DumpReport界面上的选择）
			enumDump_LogDirectory,									// 设置log文件路径
			enumDump_CollectFile,									// 设置需收集的文件名
			enumDump_LogDirectory_UTF8,								// 设置log文件路径（传入参数为UTF8，Unity/UE 使用此参数）
			enumDump_CollectFile_UTF8,								// 设置需收集的文件名（传入参数为UTF8，Unity/UE 使用此参数）
			enumDump_SetURL,										// 设置上传url (internal_cn|internal_us|external|internal_oversea) => (国内|海外|外部项目)
			enumDump_SetBeta,										// 设置 beta
			enumDump_SetUserIdentifier,								// 设置 UserIdentifier	
			enumDump_SetUserIdentifier_UTF8,						// 设置 UserIdentifier（传入参数为UTF8，Unity/UE 使用此参数）
			enumDump_AddExtraData,								    // 添加额外数据
			enumDump_AddExtraData_UTF8,								// 添加额外数据（传入参数为UTF8，Unity/UE 使用此参数）
			enumDump_leaveBreadcrumbType,						    // 设置面包屑传入字符串的格式
			enumDump_GM_TEST,										// GM测试指令

			enumDump_Count
        };

        // 面包屑传入字符串的格式
		enum leaveBreadcrumbType
		{
			leaveBreadcrumbType_begin = 0,

			leaveBreadcrumbType_other = leaveBreadcrumbType_begin,
			leaveBreadcrumbType_ANSI,
			leaveBreadcrumbType_UTF8,

			leaveBreadcrumbType_count
		};

        typedef BOOL(*pfnInitDumperCrasheyeType)(const char* strAppkey, const char* strVersion, const char* strChannel, const BOOL bHookUnHandledExceptionFilter);
        typedef void (*pfnUnInitDumperType)();
        typedef BOOL(*pfnSetOnMiniDumpCreateCallBackType)(FnOnCrashCallback pCallback);
        typedef BOOL(*pfnSetConfigType)(const int nCommandType, const void* pArg);
        typedef BOOL(*pfnSendScriptExceptionType)(const char* strErrorTitle, const char* strStackTrace, const char* strLanguage);
        typedef LONG(*pfnHandleExceptionType)(EXCEPTION_POINTERS* pExceptionInfo);
        typedef BOOL(*pfnPushLogTraceType)(const char* cpszMessage);
        typedef void (*pfnleaveBreadcrumbType)(const char* cpszMessage);

		ApiHolder() :pInitFunction(nullptr), pfnSetOnMiniDumpCreateCallBack(nullptr), pfnSetConfig(nullptr), pUnInitFunction(nullptr), pfnSendScriptException(nullptr)
            , pfnHandleException(nullptr), pfnPushLogTrace(nullptr),pfnleaveBreadcrumb(nullptr)
		{
			hDumper = LoadLibraryW(DUMPER_DLL);
			if (hDumper)
			{
				pInitFunction = (pfnInitDumperCrasheyeType)::GetProcAddress(hDumper, "InitDumperCrasheye");
				pfnSetOnMiniDumpCreateCallBack = (pfnSetOnMiniDumpCreateCallBackType)::GetProcAddress(hDumper, "SetOnMiniDumpCreateCallBack");
				pfnSetConfig = (pfnSetConfigType)::GetProcAddress(hDumper, "SetConfig");
				pUnInitFunction = (pfnUnInitDumperType)::GetProcAddress(hDumper, "UnInitDumper");
                pfnSendScriptException = (pfnSendScriptExceptionType)::GetProcAddress(hDumper, "SendScriptException");
                pfnHandleException = (pfnHandleExceptionType)::GetProcAddress(hDumper, "HandleException");
                pfnPushLogTrace = (pfnPushLogTraceType)::GetProcAddress(hDumper, "PushLogTrace");
                pfnleaveBreadcrumb = (pfnleaveBreadcrumbType)::GetProcAddress(hDumper, "leaveBreadcrumb");
			}
		}

		~ApiHolder()
		{
			if (hDumper)
			{
				if (pUnInitFunction)
				{
					pUnInitFunction();
					pUnInitFunction = nullptr;
				}
				FreeLibrary(hDumper);
				hDumper = NULL;
			}
		}

		HMODULE hDumper;
		pfnInitDumperCrasheyeType pInitFunction;
		pfnSetOnMiniDumpCreateCallBackType pfnSetOnMiniDumpCreateCallBack;
		pfnSetConfigType pfnSetConfig;
        pfnUnInitDumperType pUnInitFunction;
        pfnSendScriptExceptionType pfnSendScriptException;
        pfnHandleExceptionType pfnHandleException;
        pfnPushLogTraceType pfnPushLogTrace;
        pfnleaveBreadcrumbType pfnleaveBreadcrumb;
    };
  
    // The non-static inline function declaration refers to the same function in every translation unit (source file) that uses it.
    inline ApiHolder* get_api()
    {
        static ApiHolder s_api;
        return &s_api;
    }
  
  
    /**
    * 初始化 CrasheyeSdk. 
    * 
    * 崩溃收集相关的(如设置SEH,VEH等)一系列初始化操作.
    *
    * 本接口调用后会产生一条报活信息, 如需设置 url, 
    * 请先调用 SetURL, 再调用本接口.
    *
    * @param  strAppkey   平台申请的当前应用 appkey
    * @param  strVersion  应用当前版本号
    * @param  strChannel  应用的渠道号
    * @return 是否成功初始化
    */
    inline BOOL Init(const char* strAppkey, const char* strVersion, const char* strChannel, const BOOL bHookUnHandledExceptionFilter = FALSE)
    {
        if (get_api()->pInitFunction)
        {
            return get_api()->pInitFunction(strAppkey, strVersion, strChannel, bHookUnHandledExceptionFilter);
        }
        return FALSE;
    }

    /**
    * 反初始化 CrasheyeSdk. 
    * 
    * 释放 Init 阶段注册的异常处理器. 
    * 在程序正常退出时调用.
    *
    */
    inline void UnInit()
    {
        if (get_api()->pUnInitFunction)
        {
            get_api()->pUnInitFunction();
        }
    }

    /**
    * 设置上报路径. 
    * 
    * 目前支持 3 种路径
    *   internal_cn => 国内自研项目
    *   internal_us => 海外自研项目
    *   external    => 外部项目
    *
    * 若不设置, 默认是 external
    *
    * 若要设置, 建议在 Init 之前调用.
    *
    * @param  szUrl  internal_cn/internal_us/external 三选一.
    * @return 是否成功设置
    */
    inline BOOL SetURL(const char* szUrl)
    {
        if (get_api()->pfnSetConfig)
        {
            return get_api()->pfnSetConfig(ApiHolder::enumDump_SetURL, szUrl);
        }
        return FALSE;
    }

    /**
    * 标记当前版本为调试版本
    * 
    * 若要设置, 建议在 Init 之前调用.
    *
    * @return 是否成功设置
    */
    inline BOOL SetBeta()
    {
        if (get_api()->pfnSetConfig)
        {
            int dummy;
            return get_api()->pfnSetConfig(ApiHolder::enumDump_SetBeta, &dummy);
        }
        return FALSE;
    }

    /**
    * 设置用户id. 
    *
    * @param  szUserIdentifier  项目内部的用户id
    * @return 是否成功设置
    */
    inline BOOL SetUserIdentifier(const char* szUserIdentifier)
    {
        if (get_api()->pfnSetConfig)
        {
            return get_api()->pfnSetConfig(ApiHolder::enumDump_SetUserIdentifier, szUserIdentifier);
        }
        return FALSE;
    }

    /**
    * 设置用户id.
    * 传入参数为UTF8，Unity/UE 使用此参数
    *
    * @param  szUserIdentifier  项目内部的用户id
    * @return 是否成功设置
    */
    inline BOOL SetUserIdentifier_UTF8(const char* szUserIdentifier)
    {
        if (get_api()->pfnSetConfig)
        {
            return get_api()->pfnSetConfig(ApiHolder::enumDump_SetUserIdentifier_UTF8, szUserIdentifier);
        }
        return FALSE;
    }

    /**
    * 添加额外上报信息.
    *
    * 以键值对的形式添加额外信息, 添加的信息会被包含着崩溃报告中随报告一同上报.
    *
    * @param  szKey   键
    * @param  szValue 值
    * @return 是否成功添加
    */
    inline BOOL AddExtraData(const char* szKey, const char* szValue)
    {
        if (get_api()->pfnSetConfig)
        {
            const char* packed[2] = { szKey, szValue };
            return get_api()->pfnSetConfig(ApiHolder::enumDump_AddExtraData, packed);
        }
        return FALSE;
    }

    /**
    * 添加额外上报信息.
    * 
    * 传入参数为UTF8，Unity/UE 使用此参数
    * 
    * 以键值对的形式添加额外信息, 添加的信息会被包含着崩溃报告中随报告一同上报.
    *
    * @param  szKey   键
    * @param  szValue 值
    * @return 是否成功添加
    */
    inline BOOL AddExtraData_UTF8(const char* szKey, const char* szValue)
    {
        if (get_api()->pfnSetConfig)
        {
            const char* packed[2] = { szKey, szValue };
            return get_api()->pfnSetConfig(ApiHolder::enumDump_AddExtraData_UTF8, packed);
        }
        return FALSE;
    }

    /**
    * 添加额外上报日志(文件).
    *
    * 添加的文件会被包含着崩溃报告中随报告一同上报.
    *
    * @param  szCollectFile   需要额外收集的文件
    * @return 是否成功添加
    */
    inline BOOL AddCustomLog(const char* szCollectFile)
    {
        if (get_api()->pfnSetConfig)
        {
            return get_api()->pfnSetConfig(static_cast<int>(ApiHolder::enumDump_CollectFile), szCollectFile);
        }
        return FALSE;
    }

    /**
    * 添加额外上报日志(文件).
    * 
    * 传入参数为UTF8，Unity/UE 使用此参数
    * 
    * 添加的文件会被包含着崩溃报告中随报告一同上报.
    *
    * @param  szCollectFile   需要额外收集的文件
    * @return 是否成功添加
    */
    inline BOOL AddCustomLog_UTF8(const char* szCollectFile)
    {
        if (get_api()->pfnSetConfig)
        {
            return get_api()->pfnSetConfig(static_cast<int>(ApiHolder::enumDump_CollectFile_UTF8), szCollectFile);
        }
        return FALSE;
    }

    /**
    * 添加自定义日志文件夹.
    *
    * 最新的log文件会被包含着崩溃报告中随报告一同上报.
    *
    * @param  szCollectLogDirectory   需要收集的自定义log文件夹
    * @return 是否成功添加
    */
    inline BOOL AddCustomLogDirectory(const char* szCollectLogDirectory)
    {
        if (get_api()->pfnSetConfig)
        {
            return get_api()->pfnSetConfig(static_cast<int>(ApiHolder::enumDump_LogDirectory), szCollectLogDirectory);
        }
        return FALSE;
    }

    /**
    * 添加自定义日志文件夹.
    *
    * * 传入参数为UTF8，Unity/UE 使用此参数
    * 
    * 最新的log文件会被包含着崩溃报告中随报告一同上报.
    *
    * @param  szCollectLogDirectory   需要收集的自定义log文件夹
    * @return 是否成功添加
    */
    inline BOOL AddCustomLogDirectory_UTF8(const char* szCollectLogDirectory)
    {
        if (get_api()->pfnSetConfig)
        {
            return get_api()->pfnSetConfig(static_cast<int>(ApiHolder::enumDump_LogDirectory_UTF8), szCollectLogDirectory);
        }
        return FALSE;
    }

    /**
    * 设置Dump是否强制上报（忽略玩家在DumpReport界面上的选择）
    *
    * 若要设置, 建议在 Init 之前调用.
    *
    * @param  isForceUpload   是否设置Dump强制上报
    * @return 是否成功设置
    */
    inline BOOL SetForceUpload(BOOL isForceUpload)
    {
        if (get_api()->pfnSetConfig)
        {
            return get_api()->pfnSetConfig(ApiHolder::enumDump_ForceUpload, &isForceUpload);
        }
        return FALSE;
    }

    /**
    * 设置Dump是否后台上报（不弹出DumpReport界面）
    *
    * 若要设置, 建议在 Init 之前调用.
    *
    * @param  isBackgroundUpload   是否设置Dump后台上报
    * @return 是否成功设置
    */
    inline BOOL SetBackgroundUpload(BOOL isBackgroundUpload)
    {
        if (get_api()->pfnSetConfig)
        {
            return get_api()->pfnSetConfig(ApiHolder::enumDump_BackgroundExecutionFlag, &isBackgroundUpload);
        }
        return FALSE;
    }

    /**
    * 设置崩溃回调
    *
    * 设置崩溃发生时的回调, 回调会在报告文件写入完成后被调用.
    * 回调函数签名为 
    *   void(*FnOnCrashCallback)(BOOL bCaptureSucceed, const char* cpszCrashReportFile);
    *   // @param bCaptureSucceed      是否成功捕获
    *   // @param cpszCrashReportFile  报告文件名
    *
    * 注意: 回调发生时报告文件已经生成, 此时再调用 AddExtraData 和 AddCustomLog 添加的内
    * 容, 不会被加到报告中. 如果想崩溃发生时添加信息, 可以在初始化时调用 
    *     AddCustomLog("after_crash.log")
    * 然后在回调中, 把信息写入 after_crash.log
    * 
    * @param  szCollectFile   需要额外收集的文件
    * @return 是否成功添加
    */
    inline BOOL SetCrashCallback(FnOnCrashCallback pCallback)
    {
        if (get_api()->pfnSetOnMiniDumpCreateCallBack)
        {
            return get_api()->pfnSetOnMiniDumpCreateCallBack(pCallback);
        }
        return FALSE;
    }
     
    /**
    * 发送脚本异常
    *
    * @param errorTitle 错误的标题
    * @param stacktrace 异常的详细内容
    * @param language   脚本语言
    * @return 是否成功设置
    */
    inline BOOL SendScriptException(const char* strErrorTitle, const char* strStackTrace, const char* strLanguage) 
    {
        if (get_api()->pfnSendScriptException) 
        {
            return get_api()->pfnSendScriptException(strErrorTitle, strStackTrace, strLanguage);
        }
        return FALSE;
    }

    /**
    * 处理系统的异常指针
    *（此指针指向的结构体包含与计算机无关的异常说明和异常发生时处理器状态的特定于处理器的说明）
    * @param ExceptionInfo
    * @return 是否成功设置
    */
    inline BOOL OnCrashCallback(LPEXCEPTION_POINTERS ExceptionInfo)
    {
        if (get_api()->pfnHandleException)
        {
            return get_api()->pfnHandleException(ExceptionInfo);
        }
        return FALSE;
    }

    /**
    * 推送log信息流给dump模块（宕机时会写到dumper日志中）
    *
    * @param cpszMessage log信息流
    * @return 是否成功设置
    */
    inline BOOL PushLogTrace(const char* cpszMessage)
    {
        if (get_api()->pfnPushLogTrace)
        {
            return get_api()->pfnPushLogTrace(cpszMessage);
        }
        return FALSE;
    }

    /**
    * 推送面包屑信息流给dump模块（一般用于记录加载地图的信息域顺序）
    *
    * @param cpszMessage 面包屑信息流
    * @return 无返回值
    * 使用前必需调用SetConfig对enumDump_leaveBreadcrumbType进行设置
    * 
    */
    inline void leaveBreadcrumb(const char* cpszMessage)
    {
        if (get_api()->pfnleaveBreadcrumb)
        {
            return get_api()->pfnleaveBreadcrumb(cpszMessage);
        }
    }

    /**
    * 推送自定义的配置信息流给dump模块
    *
    * @param nCommandType 要设置的Crasheye配置选项
    * @param pArg         要设置的Crasheye配置选项的值
    * @return 是否成功设置
    */
    inline BOOL setConfig(const int nCommandType, const void* pArg)
    {
		if (get_api()->pfnSetConfig)
		{
            return get_api()->pfnSetConfig(nCommandType, pArg);
		}
        return FALSE;
    }
}

#endif // _WIN32