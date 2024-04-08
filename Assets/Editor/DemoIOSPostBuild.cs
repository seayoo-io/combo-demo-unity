#if UNITY_IOS
using UnityEditor.Build;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
using UnityEditor.Build.Reporting;
using System.Collections.Generic;

public class DemoIOSPostBuild : IPostprocessBuildWithReport
{
    public static class BuildArguments {
        public static string BundleId => System.Environment.GetEnvironmentVariable("PRODUCT_BUNDLE_IDENTIFIER");
        public static string SignIdentity => System.Environment.GetEnvironmentVariable("CODE_SIGN_IDENTITY");
        public static string Provision => System.Environment.GetEnvironmentVariable("PROVISIONING_PROFILE_SPECIFIER");
        public static string DevelopmentTeam => System.Environment.GetEnvironmentVariable("DEVELOPMENT_TEAM");
        public static string ComboSDKConfigPath => System.Environment.GetEnvironmentVariable("COMBOSDK_CONFIG_PATH");
        public static string Capabilities => System.Environment.GetEnvironmentVariable("CAPABILITIES");
    }

    public int callbackOrder { get { return 1; } }

    public void CreateDirectory(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    private static readonly string ComboSDKFrameworks = "ComboSDKFrameworks";

    public void OnPostprocessBuild(BuildReport report)
    {
        var szFrameworkPath = System.Environment.GetEnvironmentVariable("FRAMEWORK_PATH") ?? "Frameworks";
        if (report.summary.platform == BuildTarget.iOS)
        {
            string projectPath = report.summary.outputPath + "/Unity-iPhone.xcodeproj/project.pbxproj";
            PBXProject pbxProject = new PBXProject();
            pbxProject.ReadFromFile(projectPath);

            string unityMainTargetGuid = pbxProject.GetUnityMainTargetGuid();
            string unityFrameworkTargetGuid = pbxProject.GetUnityFrameworkTargetGuid();
            var unityMainFrameworksBuildPhase = pbxProject.GetFrameworksBuildPhaseByTarget(unityMainTargetGuid);
            var unityFrameworkBuildPhase = pbxProject.GetFrameworksBuildPhaseByTarget(unityFrameworkTargetGuid);

            Debug.Log($"[Demo] BuildArguments.BundleId: {BuildArguments.BundleId} success!");
            Debug.Log($"[Demo] SignIdentity: {BuildArguments.SignIdentity} success!");
            Debug.Log($"[Demo] Provision: {BuildArguments.Provision} success!");
            Debug.Log($"[Demo] DEVELOPMENT_TEAM: {BuildArguments.DevelopmentTeam} success!");

            // Add Frameworks
            AddFrameworks(szFrameworkPath, report, pbxProject, unityMainTargetGuid, unityMainFrameworksBuildPhase, unityFrameworkTargetGuid, unityFrameworkBuildPhase);

            // Build Setting
            SetBuildProperty(pbxProject, unityMainTargetGuid, unityFrameworkTargetGuid);

            // ComboSDK.json
            CopyAndAddComboSDKJson(report, pbxProject, unityMainTargetGuid);

            // Add Sign In With Apple Capability
            Debug.Log($"[Demo] BuildArguments.Capabilities: {BuildArguments.Capabilities}");
            if (!string.IsNullOrEmpty(BuildArguments.Capabilities))
            {
                string[] capabilitiesArray = BuildArguments.Capabilities.Split(',');
                foreach (string capability in capabilitiesArray)
                {
                    Debug.Log("[Demo] Capability: " + capability);
                }
                if (capabilitiesArray.Contains("SignInWithApple"))
                {
                    Debug.Log("[Demo] add signInWithApple");
                    AddAppleSignInCapability(report, pbxProject, unityMainTargetGuid);
                }
            }
            
            pbxProject.WriteToFile(projectPath);

            // Info.plist
            UpdatePListFile(report);

            Debug.Log($"[Demo] PostBuild iOS init xcodeproj finish!");
        }
    }

    private void AddFrameworks(string szFrameworkPath, BuildReport report, PBXProject pbxProject,
        string mainTargetGuid, string mainFrameworksBuildPhase, string frameworkTargetGuid, string frameworkBuildPhase)
    {
        // 扫描一级目录
        var levelOneDirectories = Directory.GetDirectories(szFrameworkPath);
        var frameworks = new Dictionary<string, List<string>>();

        foreach (var levelOneDirectory in levelOneDirectories)
        {
            var levelOneDirectoryName = Path.GetFileName(levelOneDirectory);

            // 筛选一级目录 .xcframework 和 .framework 目录
            if (levelOneDirectoryName.EndsWith(".xcframework") || levelOneDirectoryName.EndsWith(".framework"))
            {
                if (!frameworks.ContainsKey(szFrameworkPath))
                {
                    frameworks[szFrameworkPath] = new List<string>();
                }
                frameworks[szFrameworkPath].Add(levelOneDirectoryName);
            }
            else
            {
                // 扫描第二级目录
                var levelTwoDirectories = Directory.GetDirectories(levelOneDirectory);

                // 通过名称筛选出 .xcframework 和 .framework 目录
                var frameworkDirectories = levelTwoDirectories.Where(directory =>
                    directory.EndsWith(".xcframework") || directory.EndsWith(".framework")).ToArray();

                if(frameworkDirectories.Length > 0)
                {
                    frameworks[levelOneDirectory] = frameworkDirectories.Select(Path.GetFileName).ToList();
                }
            }
        }

        foreach (var pair in frameworks)
        {
            foreach (var frameworkName in pair.Value)
            {
                string destPath = Path.Combine(report.summary.outputPath, ComboSDKFrameworks, frameworkName);
                Builder.DirectoryCopy(Path.Combine(pair.Key, frameworkName), destPath);

                string fileGuid = pbxProject.AddFile(destPath, $"{ComboSDKFrameworks}/{frameworkName}", PBXSourceTree.Sdk);
                if (fileGuid != null)
                {
                    pbxProject.AddFileToEmbedFrameworks(mainTargetGuid, fileGuid);
                    pbxProject.AddFileToBuildSection(mainTargetGuid, mainFrameworksBuildPhase, fileGuid);
                    pbxProject.AddFileToBuildSection(frameworkTargetGuid, frameworkBuildPhase, fileGuid);
                    Debug.Log($"{frameworkName} add!");
                }
                else
                {
                    Debug.Log($"{frameworkName} file not found!");
                }
            }
        }
    }

    private void SetBuildProperty(PBXProject pbxProject, string mainTargetGuid, string unityFrameworkTargetGuid)
    {
        Debug.Log("[Demo] Start to SetBuildProperty");
        // Sign
        pbxProject.SetBuildProperty(mainTargetGuid, "PRODUCT_BUNDLE_IDENTIFIER", BuildArguments.BundleId);
        pbxProject.SetBuildProperty(mainTargetGuid, "CODE_SIGN_STYLE", "Manual");
        // pbxProject.SetBuildProperty(mainTargetGuid, "CODE_SIGN_IDENTITY", "iPhone Developer: fei gao (KDF9BF4JFY)");
        pbxProject.SetBuildProperty(mainTargetGuid, "PROVISIONING_PROFILE_SPECIFIER", BuildArguments.Provision);
        pbxProject.SetBuildProperty(mainTargetGuid, "DEVELOPMENT_TEAM", BuildArguments.DevelopmentTeam);
        // Framework search path
        pbxProject.SetBuildProperty(mainTargetGuid, "FRAMEWORK_SEARCH_PATHS", $"$(PROJECT_DIR)/{ComboSDKFrameworks}");
        pbxProject.SetBuildProperty(unityFrameworkTargetGuid, "FRAMEWORK_SEARCH_PATHS", $"$(PROJECT_DIR)/{ComboSDKFrameworks}");
        Debug.Log("[Demo] End to SetBuildProperty");
    }

    private void AddAppleSignInCapability(BuildReport report, PBXProject pbxProject, string mainTargetGuid)
    {
        Debug.Log("[Demo] Start to AddAppleSignInCapability");
        var entitlementsPath = $"{report.summary.outputPath}/Unity-iPhone/Unity-iPhone.entitlements";
        var entitlements = new PlistDocument();
        var array = entitlements.root.CreateArray("com.apple.developer.applesignin");
        array.AddString("Default");
        File.WriteAllText(entitlementsPath, entitlements.WriteToString());
        var relativeEntitlementsPath = "Unity-iPhone/Unity-iPhone.entitlements";
        pbxProject.AddFile(entitlementsPath, relativeEntitlementsPath);
        pbxProject.AddCapability(mainTargetGuid, PBXCapabilityType.SignInWithApple, relativeEntitlementsPath);
        Debug.Log("[Demo] End to AddAppleSignInCapability");
    }

    private void CopyAndAddComboSDKJson(BuildReport report, PBXProject pbxProject, string mainTargetGuid)
    {
        Debug.Log("[Demo] Start to CopyAndAddComboSDKJson");
        string destJsonFilePath = Path.Combine(report.summary.outputPath, ComboSDKFrameworks, "ComboSDK.json");

        Builder.CopyFile(BuildArguments.ComboSDKConfigPath, destJsonFilePath);

        var guid = pbxProject.AddFile(destJsonFilePath, $"{ComboSDKFrameworks}/ComboSDK.json", PBXSourceTree.Source);
        pbxProject.AddFileToBuild(mainTargetGuid, guid);
        Debug.Log("[Demo] End to CopyAndAddComboSDKJson");
    }

    private void UpdatePListFile(BuildReport report)
    {
        var plistPath = $"{report.summary.outputPath}/Info.plist";
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        plist.root.SetString("NSUserTrackingUsageDescription", "");
        plist.root.SetBoolean("UIFileSharingEnabled", true);
        plist.WriteToFile(plistPath);
    }
}
#endif
