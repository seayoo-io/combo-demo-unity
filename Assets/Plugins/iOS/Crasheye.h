//  [2.7.10]
//  Crasheye.h
//  Crasheye
//
//  Created by PengYuanlong on 16-12-16.
//  Copyright (c) 2014年 PengYuanlong. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface Crasheye : NSObject

/*
 @brief:通过appKey注册Dump收集
 */
+(void) initWithAppKey:(NSString *) appKey;

/*
 @brief:通过appKey注册Dump收集，并指定channel
 @param:appKey  appKey
 @param:channel 指定的channel, 由开发都自行填入
 */
+(void) initWithAppKey:(NSString *) appKey
               withChannel:(NSString *) channel;


/*
 @brief:添加自定义数据
 */
+(void) addExtraDataWithKey:(NSString *) key withValue:(NSString *) value;

/*
 @brief:添加自定义数据
 */
+(void) addExtraDataWithDic:(NSDictionary *) dic;

/*
 @brief:移除一项自定义数据
 */
+(void) removeExtraDataWithKey:(NSString *) key;

/*
 @brief:移除所有自定义数据
 */
+(void) clearExtraData;

+(void) addLog: (NSString *) log;

+(void) removeLog;

/*
 @brief:获取已添加的自定义数据
 */
+(NSDictionary *) extraData;

/*
 @brief:主动上报脚本异常，可用于lua,js等脚本异常时上报
 @param:
    errorTitle 错误的标题，由开发人员指定，不可以为空
    exception  异常的详细内容  不可以为空
 */
+(void) sendScriptExceptionRequestWithTitle:(NSString *) errorTitle exception:(NSString *) exception file:(NSString *) file language:(NSString *) language;

/*
 @brief:操作打点, 打点信息会做crash信息一同上报，只保留最后的10个打点信息
 */
+(void) leaveBreadcrumb:(NSString *) breadcrumb;

/*
 @brief:设置用户ID
 */
+(void) setUserID:(NSString *) userID;

+(void) setRegion:(NSString *) region;

/*
 @brief:设置App Version，需在init前调用
 */
+(void) setAppVersion:(NSString *) appVersion;

/*
 @brief:设置用户信息
 */
+(void) setUserInfo:(NSDictionary *) userInfo;


/*
 @brief:获取Crasheye SDK的版本号
 */
+(NSString *) versionForCrasheye;

/*
 @brief:获取设备唯一码
 */
+(NSString *) getDeviceID;

typedef void (*CrashCallback)(int nSign);
/*
 @brief:注册一个回调函数，用于发生Crash的时候，由crasheye通知client
 */
+(void) registerCrashCallback:(CrashCallback) callback;

+(void) setFlushOnlyOverWiFi:(int32_t) enabled;

+(void) setBeta:(int32_t) enabled;

+(void) setLogCount: (int32_t) count;

+(void) log:(NSString *) logMsg;

+(void) setURL:(NSString *) serverURL;

@end
