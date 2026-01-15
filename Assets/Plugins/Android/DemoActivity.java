package com.crasheye.client.api.unity3d;

import android.content.ComponentName;
import android.content.pm.ActivityInfo;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageManager;
import android.os.Bundle;
import android.util.Log;

import com.seayoo.sdk.ComboSDKMainActivity;
import com.unity3d.player.UnityPlayerActivity;
import com.xsj.crasheye.Crasheye;

public class DemoActivity extends UnityPlayerActivity {
    public static String TAG = "MonoCrasheye";
    
    String Crasheye_appkey = null;

    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        
        try {
            ActivityInfo activityInfo = getPackageManager().getActivityInfo(
                new ComponentName(this, ComboSDKMainActivity.class),
                PackageManager.GET_META_DATA
            );

            Bundle bundle = activityInfo.metaData;
            Crasheye_appkey = bundle.getString("Crasheye_appkey");
        } catch (PackageManager.NameNotFoundException e) {
            e.printStackTrace();
        }
        
        if (Crasheye_appkey == null) {
            Log.e(TAG,"appkey is Empty");
        } else {
            Crasheye.initWithMonoNativeHandle(this, Crasheye_appkey);
        }
    }
}