#if UNITY_IOS
using UnityEditor.Build;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.iOS.Xcode;
using UnityEditor.Build.Reporting;

public class DemoIOSPostBuild : IPostprocessBuildWithReport
{
    public static class BuildArguments {
        public static string BundleId => System.Environment.GetEnvironmentVariable("PRODUCT_BUNDLE_IDENTIFIER") ?? "com.seayoo.demo.combo";
        public static string SignIdentity => System.Environment.GetEnvironmentVariable("CODE_SIGN_IDENTITY") ?? "Apple Development: Hao Wu (AVMJXXS6SS)";
        public static string Provision => System.Environment.GetEnvironmentVariable("PROVISIONING_PROFILE_SPECIFIER") ?? "combo_dev_provision";
        public static string DevelopmentTeam => System.Environment.GetEnvironmentVariable("DEVELOPMENT_TEAM") ?? "3PMCL4FRNT";
        public static string GameId => System.Environment.GetEnvironmentVariable("COMBOSDK_GAME_ID") ?? "demo";
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
        pbxProject.SetBuildProperty(mainTargetGuid, "PRODUCT_BUNDLE_IDENTIFIER", BuildArguments.BundleId);
    }

    private void UpdatePListFile(BuildReport report)
    {
        var plistPath = $"{report.summary.outputPath}/Info.plist";
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        plist.root.SetBoolean("UIFileSharingEnabled", true);
        plist.root.SetString("CFBundleDisplayName", BuildArguments.GameId);
        plist.WriteToFile(plistPath);
    }

}
#endif
