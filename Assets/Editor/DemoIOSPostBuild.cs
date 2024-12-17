﻿#if UNITY_IOS
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
        public static string BundleId => System.Environment.GetEnvironmentVariable("PRODUCT_BUNDLE_IDENTIFIER") ?? "com.seayoo.demo.combo";
        public static string SignIdentity => System.Environment.GetEnvironmentVariable("CODE_SIGN_IDENTITY") ?? "Apple Development: Hao Wu (AVMJXXS6SS)";
        public static string Provision => System.Environment.GetEnvironmentVariable("PROVISIONING_PROFILE_SPECIFIER") ?? "combo_dev_provision";
        public static string DevelopmentTeam => System.Environment.GetEnvironmentVariable("DEVELOPMENT_TEAM") ?? "3PMCL4FRNT";
        public static string Capabilities => System.Environment.GetEnvironmentVariable("CAPABILITIES") ?? "SignInWithApple";
    }

    public int callbackOrder { get { return 1; } }

    public void CreateDirectory(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    private static readonly string ComboSDKDirectory = "ComboSDK";

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.iOS)
        {
            string projectPath = report.summary.outputPath + "/Unity-iPhone.xcodeproj/project.pbxproj";
            PBXProject pbxProject = new PBXProject();
            pbxProject.ReadFromFile(projectPath);

            string unityMainTargetGuid = pbxProject.GetUnityMainTargetGuid();
            string unityFrameworkTargetGuid = pbxProject.GetUnityFrameworkTargetGuid();

            // Build Setting
            SetBuildProperty(pbxProject, unityMainTargetGuid);

            // Add Selected Capability
            Debug.Log($"[Demo] BuildArguments.Capabilities: {BuildArguments.Capabilities}");
            if (!string.IsNullOrEmpty(BuildArguments.Capabilities))
            {
                string[] capabilitiesArray = BuildArguments.Capabilities.Split(',');
                if (capabilitiesArray.Contains("SignInWithApple"))
                {
                    AddAppleSignInCapability(report, pbxProject, unityMainTargetGuid);
                }
                if (capabilitiesArray.Contains("PushNotifications"))
                {
                    AddPushNotificationsCapability(report, pbxProject, unityMainTargetGuid);
                }
            }

            // UNITY_USES_REMOTE_NOTIFICATIONS 0 -> 1
            PatchPreprocessor(report.summary.outputPath);

            // PrivacyInfo.xcprivacy
            var privacyInfoPath = "Assets/Plugins/iOS/PrivacyInfo.xcprivacy";
            AddPrivacyInfo(report, pbxProject, unityFrameworkTargetGuid, privacyInfoPath);

            pbxProject.WriteToFile(projectPath);

            // Info.plist
            UpdatePListFile(report);

            Debug.Log($"[Demo] PostBuild iOS init xcodeproj finish!");
        }
    }

    private void SetBuildProperty(PBXProject pbxProject, string mainTargetGuid)
    {
        Debug.Log("[Demo] Start to SetBuildProperty " + "developmentTeam: " + BuildArguments.DevelopmentTeam + " signIdentity: " + BuildArguments.SignIdentity + " provision: " + BuildArguments.Provision);
        // Sign
        pbxProject.SetBuildProperty(mainTargetGuid, "PRODUCT_BUNDLE_IDENTIFIER", BuildArguments.BundleId);
        pbxProject.SetBuildProperty(mainTargetGuid, "CODE_SIGN_STYLE", "Manual");
        pbxProject.SetBuildProperty(mainTargetGuid, "CODE_SIGN_IDENTITY", BuildArguments.SignIdentity);
        pbxProject.SetBuildProperty(mainTargetGuid, "PROVISIONING_PROFILE_SPECIFIER", BuildArguments.Provision);
        pbxProject.SetBuildProperty(mainTargetGuid, "DEVELOPMENT_TEAM", BuildArguments.DevelopmentTeam);
    }

    private void AddAppleSignInCapability(BuildReport report, PBXProject pbxProject, string mainTargetGuid)
    {
        Debug.Log("[Demo] Start to AddAppleSignInCapability");
        var entitlementsPath = $"{report.summary.outputPath}/Unity-iPhone/Unity-iPhone.entitlements";
        var entitlements = new PlistDocument();

        if (File.Exists(entitlementsPath))
        {
            entitlements.ReadFromFile(entitlementsPath);
        }
        else
        {
            entitlements.root.CreateArray("com.apple.developer.applesignin");
        }

        var rootDict = entitlements.root.AsDict();
        PlistElementArray array;
        if (rootDict.values.ContainsKey("com.apple.developer.applesignin"))
        {
            array = rootDict.values["com.apple.developer.applesignin"].AsArray();
        }
        else
        {
            array = rootDict.CreateArray("com.apple.developer.applesignin");
        }
        if (!array.values.Any(value => value.AsString() == "Default"))
        {
            array.AddString("Default");
        }

        File.WriteAllText(entitlementsPath, entitlements.WriteToString());
        var relativeEntitlementsPath = "Unity-iPhone/Unity-iPhone.entitlements";
        pbxProject.AddFile(entitlementsPath, relativeEntitlementsPath);
        pbxProject.AddCapability(mainTargetGuid, PBXCapabilityType.SignInWithApple, relativeEntitlementsPath);
    }

    private void AddPushNotificationsCapability(BuildReport report, PBXProject pbxProject, string mainTargetGuid)
    {
        Debug.Log("[Demo] Start to PushNotifications");
        var entitlementsPath = $"{report.summary.outputPath}/Unity-iPhone/Unity-iPhone.entitlements";
        var entitlements = new PlistDocument();
        
        if (File.Exists(entitlementsPath))
        {
            entitlements.ReadFromFile(entitlementsPath);

            var rootDict = entitlements.root.AsDict();
            if (!rootDict.values.ContainsKey("aps-environment"))
            {
                entitlements.root.SetString("aps-environment", "development");
            }
        }
        else
        {
            entitlements.root.SetString("aps-environment", "development");
        }

        File.WriteAllText(entitlementsPath, entitlements.WriteToString());
        var relativeEntitlementsPath = "Unity-iPhone/Unity-iPhone.entitlements";
        pbxProject.AddFile(entitlementsPath, relativeEntitlementsPath);
        pbxProject.AddCapability(mainTargetGuid, PBXCapabilityType.SignInWithApple, relativeEntitlementsPath);
    }

    private void UpdatePListFile(BuildReport report)
    {
        var plistPath = $"{report.summary.outputPath}/Info.plist";
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        plist.root.SetBoolean("UIFileSharingEnabled", true);
        plist.WriteToFile(plistPath);
    }

    private void AddPrivacyInfo(BuildReport report, PBXProject pbxProject, string unityFrameworkTargetGuid, string privacyPath)
    {
        string destJsonFilePath = Path.Combine(report.summary.outputPath, ComboSDKDirectory, "PrivacyInfo.xcprivacy");
        Debug.Log($"PrivacyInfo.xcprivacy = {destJsonFilePath}");
        Builder.CopyFile(privacyPath, destJsonFilePath);
        string projPathOfFile = $"{ComboSDKDirectory}/PrivacyInfo.xcprivacy";
        var guid = pbxProject.AddFile(projPathOfFile, projPathOfFile, PBXSourceTree.Source);
        pbxProject.AddFileToBuild(unityFrameworkTargetGuid, guid);
    }

    // UNITY_USES_REMOTE_NOTIFICATIONS 0 -> 1
    private void PatchPreprocessor(string path)
    {
        var preprocessorPath = path + "/Classes/Preprocessor.h";
        Debug.Log($"preprocessorPath = {preprocessorPath}");
        var preprocessor = File.ReadAllText(preprocessorPath);
        preprocessor = preprocessor.Replace("UNITY_USES_REMOTE_NOTIFICATIONS 0", "UNITY_USES_REMOTE_NOTIFICATIONS 1");
        File.WriteAllText(preprocessorPath, preprocessor);
    }
}
#endif
