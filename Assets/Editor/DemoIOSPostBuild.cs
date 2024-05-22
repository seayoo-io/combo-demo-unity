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
        public static string BundleId => System.Environment.GetEnvironmentVariable("PRODUCT_BUNDLE_IDENTIFIER") ?? "com.ksDemo.omni";
        public static string SignIdentity => System.Environment.GetEnvironmentVariable("CODE_SIGN_IDENTITY") ?? "Apple Development: TingTing Liu (QWVQYB57WJ)";
        public static string Provision => System.Environment.GetEnvironmentVariable("PROVISIONING_PROFILE_SPECIFIER") ?? "dev_provision";
        public static string DevelopmentTeam => System.Environment.GetEnvironmentVariable("DEVELOPMENT_TEAM") ?? "SP537S8Q2J";
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
        if (report.summary.platform == BuildTarget.iOS)
        {
            string projectPath = report.summary.outputPath + "/Unity-iPhone.xcodeproj/project.pbxproj";
            PBXProject pbxProject = new PBXProject();
            pbxProject.ReadFromFile(projectPath);

            string unityMainTargetGuid = pbxProject.GetUnityMainTargetGuid();
            string unityFrameworkTargetGuid = pbxProject.GetUnityFrameworkTargetGuid();

            // Build Setting
            SetBuildProperty(pbxProject, unityMainTargetGuid);

            // Add Sign In With Apple Capability
            AddAppleSignInCapability(report, pbxProject, unityMainTargetGuid);

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
        // Sign
        pbxProject.SetBuildProperty(mainTargetGuid, "PRODUCT_BUNDLE_IDENTIFIER", BuildArguments.BundleId);
        pbxProject.SetBuildProperty(mainTargetGuid, "CODE_SIGN_STYLE", "Manual");
        pbxProject.SetBuildProperty(mainTargetGuid, "CODE_SIGN_IDENTITY", BuildArguments.SignIdentity);
        pbxProject.SetBuildProperty(mainTargetGuid, "PROVISIONING_PROFILE_SPECIFIER", BuildArguments.Provision);
        pbxProject.SetBuildProperty(mainTargetGuid, "DEVELOPMENT_TEAM", BuildArguments.DevelopmentTeam);
    }

    private void AddAppleSignInCapability(BuildReport report, PBXProject pbxProject, string mainTargetGuid)
    {
        var entitlementsPath = $"{report.summary.outputPath}/Unity-iPhone/Unity-iPhone.entitlements";
        var entitlements = new PlistDocument();
        // Add Sign In With Apple Capability
        var array = entitlements.root.CreateArray("com.apple.developer.applesignin");
        array.AddString("Default");
        // Add Push Notification Capability
        entitlements.root.SetString("aps-environment", "development");
        File.WriteAllText(entitlementsPath, entitlements.WriteToString());
        var relativeEntitlementsPath = "Unity-iPhone/Unity-iPhone.entitlements";
        pbxProject.AddFile(entitlementsPath, relativeEntitlementsPath);
        pbxProject.AddCapability(mainTargetGuid, PBXCapabilityType.SignInWithApple, relativeEntitlementsPath);
        pbxProject.AddCapability(mainTargetGuid, PBXCapabilityType.PushNotifications, relativeEntitlementsPath);
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
        string destJsonFilePath = Path.Combine(report.summary.outputPath, ComboSDKFrameworks, "PrivacyInfo.xcprivacy");
        Debug.Log($"PrivacyInfo.xcprivacy = {destJsonFilePath}");
        Builder.CopyFile(privacyPath, destJsonFilePath);
        string projPathOfFile = $"{ComboSDKFrameworks}/PrivacyInfo.xcprivacy";
        var guid = pbxProject.AddFile(projPathOfFile, projPathOfFile, PBXSourceTree.Source);
        pbxProject.AddFileToBuild(unityFrameworkTargetGuid, guid);
    }
}
#endif
