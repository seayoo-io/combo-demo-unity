using System;
using System.IO;
using Combo;
using UnityEditor;
using UnityEngine;

[Serializable]
public class BuildParams
{
    public string demoEndpoint = null; // demo server endpoint
    public string branchName = null;
    public string buildNumber = null;
    public bool forceUpdate = false;
    public bool hotUpdate = false;
    public string gameId = null;
    public string buildKey = null;
    public string comboEndpoint = null;
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
            gameId = Environment.GetEnvironmentVariable("COMBOSDK_GAME_ID"),
            buildKey = Environment.GetEnvironmentVariable("COMBOSDK_BUILD_KEY"),
            comboEndpoint = Environment.GetEnvironmentVariable("COMBOSDK_ENDPOINT"),
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

    public static string GetGameId()
    {
#if UNITY_EDITOR
        return EditorPrefs.GetString("COMBOSDK_GAME_ID", "");
#else
        var gameId = BuildParams.Load().gameId;
        if (string.IsNullOrEmpty(gameId))
        {
            gameId = Combo.Configuration.Instance.GameId;
        }
        return gameId;
#endif
    }

    public static string GetBuildKey()
    {
#if UNITY_EDITOR
        return EditorPrefs.GetString("BUILD_KEY", ""); // Run for editor
#else
        var buildKey = BuildParams.Load().buildKey; // Build for editor
        if (string.IsNullOrEmpty(buildKey))
        {
            buildKey = Combo.Configuration.Instance.BuildKey; // Build for jenkins
        }
        return buildKey;
#endif
    }

    public static string GetComboSDKEndpoint()
    {
#if UNITY_EDITOR
        return EditorPrefs.GetString("COMBOSDK_ENDPOINT", "");
#else
        var endpoint = BuildParams.Load().comboEndpoint;
        if (string.IsNullOrEmpty(endpoint))
        {
            endpoint = Combo.Configuration.Instance.Endpoint;
        }
        return endpoint;
#endif
    }
}