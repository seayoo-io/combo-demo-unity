using System;
using System.IO;
using System.Xml;
using UnityEditor.Android;
using UnityEngine;

/// <summary>
/// SCREEN_ORIENTATION=SensorLandscape 时，将导出工程中 UnityPlayerActivity 的 screenOrientation 固定为 sensorLandscape。
/// Unity 对「自动旋转 + 仅横屏」默认生成 userLandscape，会受系统旋转锁影响，与 sensorLandscape 行为不同。
/// </summary>
public class AndroidOrientationPostprocessor : IPostGenerateGradleAndroidProject
{
    const string AndroidNs = "http://schemas.android.com/apk/res/android";

    public int callbackOrder => 0;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        if (Environment.GetEnvironmentVariable("SCREEN_ORIENTATION") != "SensorLandscape")
            return;

        // path 指向 unityLibrary 模块目录
        var manifestPath = Path.Combine(path, "src", "main", "AndroidManifest.xml");
        if (!File.Exists(manifestPath))
        {
            Debug.LogWarning($"[AndroidOrientationPostprocessor] AndroidManifest.xml not found: {manifestPath}");
            return;
        }

        var doc = new XmlDocument();
        doc.Load(manifestPath);
        var nsMgr = new XmlNamespaceManager(doc.NameTable);
        nsMgr.AddNamespace("android", AndroidNs);

        var activities = doc.SelectNodes("/manifest/application/activity[@android:screenOrientation]", nsMgr);
        foreach (XmlElement activity in activities)
            activity.SetAttribute("screenOrientation", AndroidNs, "sensorLandscape");

        doc.Save(manifestPath);
        Debug.Log($"[AndroidOrientationPostprocessor] set screenOrientation=sensorLandscape on {activities.Count} activity(s)");
    }
}
