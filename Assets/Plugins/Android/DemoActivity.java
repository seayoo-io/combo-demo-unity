package com.crasheye.client.api.unity3d;

import android.content.pm.ApplicationInfo;
import android.content.pm.PackageManager;
import android.os.Bundle;
import android.util.Log;

import com.unity3d.player.UnityPlayerActivity;
import com.xsj.crasheye.Crasheye;

public class DemoActivity extends UnityPlayerActivity {
    public static String TAG = "MonoCrasheye";
    
    String Crasheye_appkey = null;

    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        
        try {
            ApplicationInfo info = getPackageManager().getApplicationInfo(getPackageName(), PackageManager.GET_META_DATA);
            Crasheye_appkey = info.metaData.getString("Crasheye_appkey");
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