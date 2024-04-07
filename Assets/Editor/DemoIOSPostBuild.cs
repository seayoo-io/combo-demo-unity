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
    }

    public int callbackOrder { get { return 1; } }

    public void CreateDirectory(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

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
            SetBuildProperty(pbxProject, unityMainTargetGuid, unityFrameworkTargetGuid);

            // Add Sign In With Apple Capability
            AddAppleSignInCapability(report, pbxProject, unityMainTargetGuid);

            pbxProject.WriteToFile(projectPath);

            Debug.Log($"[Demo] PostBuild iOS init xcodeproj finish!");
        }
    }

    private void SetBuildProperty(PBXProject pbxProject, string mainTargetGuid, string unityFrameworkTargetGuid)
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
        var array = entitlements.root.CreateArray("com.apple.developer.applesignin");
        array.AddString("Default");
        File.WriteAllText(entitlementsPath, entitlements.WriteToString());
        var relativeEntitlementsPath = "Unity-iPhone/Unity-iPhone.entitlements";
        pbxProject.AddFile(entitlementsPath, relativeEntitlementsPath);
        pbxProject.AddCapability(mainTargetGuid, PBXCapabilityType.SignInWithApple, relativeEntitlementsPath);
    }
}
#endif
