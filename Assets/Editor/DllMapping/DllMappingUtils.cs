/*
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

using UnityEditor;
using Newtonsoft.Json;

public class DllMappingUtils
{
    public const string DllSeayooAccount = "ComboSDK-SeayooAccount.dll";
    public const string DllComboSDK = "ComboSDK-Windows.dll";

    [MenuItem("ComboSDK/Generate/DllMapping File")]
    public static void GenerateDllMapping()
    {
        var accountMapping = GenerateSeayooAccount();
        var foundationMapping = GenerateSeayooFoundation();
        var newMapping = accountMapping
        .Concat(foundationMapping)
        .GroupBy(kvp => kvp.Key)
        .ToDictionary(group => group.Key, group => group.Last().Value);

        if (newMapping.Count == 0)
        {
            Debug.LogError("Failed to GenerateDllMapping, mapping is empty.");
        }

        var rootDir = GetRootDir();
        var jsonContent = JsonConvert.SerializeObject(newMapping);
        var jsonPath = Path.Combine(rootDir, "mapping.json");
        File.WriteAllText(jsonPath, jsonContent, System.Text.Encoding.UTF8);
        Debug.LogWarning("Finish to generate mapping file.");
    }

    public static void RefreshPrefabs(string prefabRootPath)
    {
        var filePath = GetMappingFilepath();
        var json = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
        var mapping = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

        var prefabs = new Dictionary<string, List<string>>()
            {
                {DllSeayooAccount, new List<string>() {"SeayooAccount.prefab"}},
                {DllComboSDK, new List<string>() {"SeayooAlert.prefab", "SeayooToaster.prefab"}}
            };

        foreach (var item in prefabs)
        {
            var dllmeta = GetDllMetaFullpath(item.Key);
            var dllGUID = DllMapping.GetGuidFromMeta(dllmeta);

            if (string.IsNullOrEmpty(dllGUID))
            {
                Debug.LogError($"Failed to dll mapping, path: {dllmeta}, reason: dll guid is empty");
                return;
            }

            item.Value.ForEach(prefabName =>
            {
                var fullpath = Path.Combine(prefabRootPath, prefabName);
                DllMapping.ReplaceScriptsFromDll(fullpath, mapping, dllGUID);
            });
        }

        AssetDatabase.Refresh();
    }

    private static Dictionary<string, string> GenerateSeayooAccount()
    {
        var dllFilepath = GetUnityGenerateDllFullPath(DllSeayooAccount);
        var codeFilePath = Path.Combine(GetRootDir(), "Packages", "com.seayoo.sdk.windows", "SeayooAccount", "View");

        var dllMapping = DllMapping.GetFielIDMappingFromDll(dllFilepath);
        var codeMapping = DllMapping.GetGUIDMappingFromCode(codeFilePath);

        var mapping = new Dictionary<string, string>();

        foreach (var kvp in codeMapping)
        {
            if (dllMapping.ContainsKey(kvp.Value))
            {
                mapping.Add(kvp.Key, dllMapping[kvp.Value]);
            }
        }
        return mapping;
    }

    private static Dictionary<string, string> GenerateSeayooFoundation()
    {
        var dllFilepath = GetUnityGenerateDllFullPath(DllComboSDK);
        var codeFilePath = Path.Combine(GetRootDir(), "Packages", "com.seayoo.sdk.windows");

        var dllMapping = DllMapping.GetFielIDMappingFromDll(dllFilepath);
        var codeMapping = DllMapping.GetGUIDMappingFromCode(codeFilePath);

        var mapping = new Dictionary<string, string>();

        foreach (var kvp in codeMapping)
        {
            // TODO: By Class Attributes
            if (kvp.Value == "Alert" || kvp.Value == "ToasterView")
            {
                Debug.Log(kvp.Key + "," + kvp.Value);
                if (dllMapping.ContainsKey(kvp.Value))
                {
                    mapping.Add(kvp.Key, dllMapping[kvp.Value]);
                }
            }
        }
        return mapping;
    }

    private static string GetMappingFilepath()
    {
        return Path.Combine(GetRootDir(), "mapping.json");
    }

    private static string GetPrefabFullpath(string prefabName)
    {
        return Path.Combine(GetRootDir(), "Packages", "com.seayoo.sdk", "Resources", "Prefabs", prefabName);
    }

    private static string GetDllMetaFullpath(string dllName)
    {
        return Path.Combine(GetRootDir(), "MetaInfo", dllName + ".meta");
    }

    // Dll generate by unity
    public static string GetUnityGenerateDllFullPath(string dllName)
    {
        var fullpath = Path.Combine(GetRootDir(), "Bin", dllName);
        if (!File.Exists(fullpath))
        {
            Debug.LogWarning("Use Library/ScriptAssemblies As dll search path");
            return Path.Combine(GetRootDir(), "Library", "ScriptAssemblies", dllName);
        }
        return fullpath;
    }

    private static string GetRootDir()
    {
        string projectRootPath = Path.GetFullPath(Application.dataPath);
        return Path.GetDirectoryName(projectRootPath);
    }
}
*/