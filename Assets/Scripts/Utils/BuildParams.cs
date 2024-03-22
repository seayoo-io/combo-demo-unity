using System;
using System.IO;
using UnityEngine;

[Serializable]
public class BuildParams
{
    public string demoEndpoint = null; // demo server endpoint
    public string branchName = null;
    public string buildNumber = null;
    public bool forceUpdate = false;
    public bool hotUpdate = false;
    private static string filePath = "Assets/Resources/ComboSDKBuildParams.json";
    private static BuildParams instance = null;

    public static void Save()
    {
        var paramz = new BuildParams
        {
            demoEndpoint = Environment.GetEnvironmentVariable("DEMO_ENDPOINT"),
            branchName = Environment.GetEnvironmentVariable("BRANCH_NAME"),
            buildNumber = Environment.GetEnvironmentVariable("BUILD_NUMBER"),
            forceUpdate = Environment.GetEnvironmentVariable("CHECK_UPDATE") == "FORCE_UPDATE",
            hotUpdate = Environment.GetEnvironmentVariable("CHECK_UPDATE") == "HOT_UPDATE",
        };
        var json = JsonUtility.ToJson(paramz);
        File.WriteAllText(filePath, json);

        Log.I($"Build params saved: {json}");
    }

    public static BuildParams Load()
    {
        if (instance != null) return instance;

        try
        {
            TextAsset jsonTextAsset = Resources.Load<TextAsset>("ComboSDKBuildParams");
            instance = JsonUtility.FromJson<BuildParams>(jsonTextAsset.text);
            Log.I("Build params loaded: " + jsonTextAsset.text);
            return instance;
        }
        catch (Exception)
        {
            instance = new BuildParams();
            Log.W("Failed to load build params");
            return instance;
        }
    }

    public static void Clear()
    {
        Log.I("Build params cleared");
        if (!File.Exists(filePath))
            return;
        File.Delete(filePath);
    }
}