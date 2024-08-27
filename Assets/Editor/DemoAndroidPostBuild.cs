#if UNITY_ANDROID
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Android;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class DemoAndroidPostBuild : IPostGenerateGradleAndroidProject
{
    public int callbackOrder { get { return 1; } }

    ProjectPath path;

    public void OnPostGenerateGradleAndroidProject(string exportPath)
    {
        exportPath = Path.GetDirectoryName(exportPath);
        Log($"Start post build for Android: {exportPath}");
        path = new ProjectPath(exportPath);
        UpdateGradle();
    }

    public void UpdateGradle()
    {

        Gradle.Load(path.LibraryGradlePath)
            .RemoveLine(s => s.Contains("--enable-debugger"))
            .RemoveLine(s => s.Contains("--profiler-report"))
            .RemoveLine(s => s.Contains("--profiler-output-file="))
            .RemoveLine(s => s.Contains("--print-command-line"))
            .Save();
        Log($"Delete more commandLines\"\n  -> {path.LibraryGradlePath}");
    }

    internal class Gradle
    {
        string srcPath;
        int ctx = 0;
        List<string> content;

        internal static Gradle Load(string path)
        {
            return new Gradle
            {
                content = new List<string>(File.ReadAllLines(path)),
                srcPath = path
            };
        }
        internal Gradle Save(string path = null) {
            if (string.IsNullOrEmpty(path))
                path = srcPath;
            File.WriteAllLines(path, content);
            return this;
        }
        internal Gradle AppendLine(string value)
        {
            content.Add(value);
            return this;
        }
        internal Gradle UpdateLine(Func<string, string> func) {
            GetContext(out string line);
            var newLine = func(line);
            content[ctx] = newLine;
            return this;
        }
        internal Gradle SelectLine(Predicate<string> predicate)
        {
            ctx = content.FindIndex(predicate);
            Assert(ctx != -1, $"Failed to select line {predicate} in {srcPath}");
            return this;
        }
        internal Gradle RemoveLine(Predicate<string> predicate)
        {
            var idx = content.FindIndex(predicate);
            if (idx != -1) {
                content.RemoveAt(idx);
            }
            return this;
        }
        Gradle GetContext(out string value) {
            value = content[ctx];
            return this;
        }
    }

    internal class ProjectPath
    {
        internal string RootPath { get; }

        internal ProjectPath(string path)
        {
            RootPath = path;
        }

        internal string LauncherSourcePath
        {
            get { return Path.Combine(RootPath, "launcher/src/main"); }
        }
        internal string LibrarySourcePath
        {
            get { return Path.Combine(RootPath, "unityLibrary/src/main"); }
        }
        internal string LauncherManifestPath
        {
            get { return Path.Combine(LauncherSourcePath, "AndroidManifest.xml"); }
        }
        internal string LibraryManifestPath
        {
            get { return Path.Combine(LibrarySourcePath, "AndroidManifest.xml"); }
        }
        internal string LauncherGradlePath
        {
            get { return Path.Combine(RootPath, "launcher/build.gradle"); }
        }
        internal string LibraryGradlePath
        {
            get { return Path.Combine(RootPath, "unityLibrary/build.gradle"); }
        }
        internal string RootGradlePath
        {
            get { return Path.Combine(RootPath, "build.gradle"); }
        }
        internal string GradlePropsPath
        {
            get { return Path.Combine(RootPath, "gradle.properties"); }
        }
    }

    static void Assert(bool condition, object message) {
        Debug.Assert(condition, message);
        if (!condition) throw new Exception($"Assert failed: {message}");
    }

    static void Log(object message) => Debug.Log($"[ComboSDK] AndroidPostBuilder: {message}");
}
#endif