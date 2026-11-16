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

    // 需要移植 UIScene 的 Unity 版本前缀（移植文件基于 2022.3.62f3c1 的 Trampoline 修改）
    private static readonly string UISceneBackportUnityVersionPrefix = "2022.3.";
    // 相对 Unity 工程根目录，移植文件平铺存放
    private static readonly string UISceneBackportDirectory = "UISceneBackport";
    // 移植文件在导出的 Xcode 工程中的路径，源文件按文件名从 UISceneBackportDirectory 中获取
    private static readonly string[] UISceneBackportFiles =
    {
        "Classes/UI/UnityScene.h",
        "Classes/UI/UnityScene.mm",
        "Classes/UnityAppController.h",
        "Classes/UnityAppController.mm",
        "Classes/Unity/DisplayManager.mm",
    };
    private static readonly string[] UISceneNewFiles =
    {
        "Classes/UI/UnityScene.h",
        "Classes/UI/UnityScene.mm",
    };

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

            // UIScene Backport
            bool uiSceneBackportEnabled = Application.unityVersion.StartsWith(UISceneBackportUnityVersionPrefix) && !IsUISceneSupportedByEngine();
            if (uiSceneBackportEnabled)
            {
                ApplyUISceneBackport(report.summary.outputPath, pbxProject, unityFrameworkTargetGuid);
            }
            else
            {
                Debug.Log($"[Demo] Skip UIScene backport, unityVersion: {Application.unityVersion}");
            }

            pbxProject.WriteToFile(projectPath);

            // Info.plist
            UpdatePListFile(report, uiSceneBackportEnabled);

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

    private void UpdatePListFile(BuildReport report, bool uiSceneBackportEnabled)
    {
        var plistPath = $"{report.summary.outputPath}/Info.plist";
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        plist.root.SetBoolean("UIFileSharingEnabled", true);
        plist.root.SetString("CFBundleDisplayName", BuildArguments.GameId);
        if (uiSceneBackportEnabled)
        {
            SetSceneManifest(plist);
        }
        plist.WriteToFile(plistPath);
    }

    // 引擎 Trampoline 模板中已包含 UnityScene（2022.3.72f1 起）时，引擎已支持 UIScene，无需移植
    private bool IsUISceneSupportedByEngine()
    {
        string playbackEngineDirectory = BuildPipeline.GetPlaybackEngineDirectory(BuildTarget.iOS, BuildOptions.None);
        return File.Exists(Path.Combine(playbackEngineDirectory, "Trampoline/Classes/UI/UnityScene.mm"));
    }

    // 用移植了 UIScene 的文件覆盖导出的 Trampoline 文件，并将新增的 UnityScene 文件加入 UnityFramework target
    private void ApplyUISceneBackport(string outputPath, PBXProject pbxProject, string frameworkTargetGuid)
    {
        string backportDirectory = Path.Combine(Directory.GetParent(Application.dataPath).FullName, UISceneBackportDirectory);
        foreach (string file in UISceneBackportFiles)
        {
            string sourcePath = Path.Combine(backportDirectory, Path.GetFileName(file));
            if (!File.Exists(sourcePath))
            {
                throw new BuildFailedException($"[Demo] UIScene backport file not found: {sourcePath}");
            }
            File.Copy(sourcePath, Path.Combine(outputPath, file), true);
        }

        foreach (string file in UISceneNewFiles)
        {
            // 追加导出（Append）时文件已在工程中，跳过避免重复添加
            if (!string.IsNullOrEmpty(pbxProject.FindFileGuidByProjectPath(file)))
            {
                continue;
            }
            string fileGuid = pbxProject.AddFile(file, file, PBXSourceTree.Source);
            pbxProject.AddFileToBuild(frameworkTargetGuid, fileGuid);
        }
        Debug.Log($"[Demo] Apply UIScene backport finish, unityVersion: {Application.unityVersion}");
    }

    // 声明 UnityScene 为 Scene 代理，与支持 UIScene 的 Unity 版本导出的 Info.plist 保持一致
    private void SetSceneManifest(PlistDocument plist)
    {
        PlistElementDict sceneManifest = plist.root.CreateDict("UIApplicationSceneManifest");
        sceneManifest.SetBoolean("UIApplicationSupportsMultipleScenes", false);
        PlistElementArray applicationRole = sceneManifest.CreateDict("UISceneConfigurations").CreateArray("UIWindowSceneSessionRoleApplication");
        PlistElementDict sceneConfiguration = applicationRole.AddDict();
        sceneConfiguration.SetString("UISceneConfigurationName", "Default Configuration");
        sceneConfiguration.SetString("UISceneDelegateClassName", "UnityScene");
    }

}
#endif
