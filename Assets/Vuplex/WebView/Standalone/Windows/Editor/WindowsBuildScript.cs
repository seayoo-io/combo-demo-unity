// Copyright (c) 2024 Vuplex Inc. All rights reserved.
//
// Licensed under the Vuplex Commercial Software Library License, you may
// not use this file except in compliance with the License. You may obtain
// a copy of the License at
//
//     https://vuplex.com/commercial-library-license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#if UNITY_STANDALONE_WIN
#pragma warning disable CS0618
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Rendering;
using Vuplex.WebView.Internal;

namespace Vuplex.WebView.Editor {

    /// <summary>
    /// Windows build script that copies the Chromium plugin executable's files to the
    /// required location in the built application folder.
    /// </summary>
    public class WindowsBuildScript : IPreprocessBuild {

        public int callbackOrder { get => 0; }

        public void OnPreprocessBuild(BuildTarget buildTarget, string buildPath) {

            if (buildTarget != BuildTarget.StandaloneWindows) {
                return;
            }
            #if !VUPLEX_DISABLE_GRAPHICS_API_WARNING
                var selectedGraphicsApi = PlayerSettings.GetGraphicsAPIs(buildTarget)[0];
                var error = VXUtils.GetGraphicsApiErrorMessage(selectedGraphicsApi, new GraphicsDeviceType[] { GraphicsDeviceType.Direct3D11 });
                if (error != null) {
                    throw new BuildFailedException(error);
                }
            #endif
        }

        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject) {

            if (!(target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64)) {
                return;
            }
            var buildPluginDirectoryPath = _getBuiltPluginDirectoryPath(target, pathToBuiltProject);
            var sourceChromiumDirectory = EditorUtils.FindDirectory(Path.Combine(new string[] { Application.dataPath, "Vuplex", "WebView", "Standalone", "Windows", "Plugins", CHROMIUM_DIRECTORY_NAME }));
            var destinationChromiumDirectory = Path.Combine(buildPluginDirectoryPath, CHROMIUM_DIRECTORY_NAME);
            EditorUtils.CopyAndReplaceDirectory(sourceChromiumDirectory, destinationChromiumDirectory);
            // Don't include the developer's Chromium log in the built app.
            var chromiumLogFilePath = Path.Combine(destinationChromiumDirectory, "log-chromium.txt~");
            if (File.Exists(chromiumLogFilePath)) {
                File.Delete(chromiumLogFilePath);
            }
        }

        const string DLL_FILE_NAME = "VuplexWebViewWindows.dll";
        const string CHROMIUM_DIRECTORY_NAME = "VuplexWebViewChromium";

        static string _getBuiltPluginDirectoryPath(BuildTarget target, string pathToBuiltProject) {

            var productName = Path.GetFileNameWithoutExtension(pathToBuiltProject);
            var buildDirectoryPath = _getParentDirectoryOfFile(pathToBuiltProject, '/');
            var architecture = target == BuildTarget.StandaloneWindows64 ? "x86_64" : "x86";
            var expectedPluginPath = Path.Combine(buildDirectoryPath, productName + "_Data", "Plugins", architecture, DLL_FILE_NAME);
            var actualPluginPath = EditorUtils.FindFile(expectedPluginPath, buildDirectoryPath);
            return _getParentDirectoryOfFile(actualPluginPath, Path.DirectorySeparatorChar);
        }

        static string _getParentDirectoryOfFile(string filePath, char pathSeparator) {

            var pathComponents = filePath.Split(new char[] { pathSeparator }).ToList();
            return String.Join(Path.DirectorySeparatorChar.ToString(), pathComponents.GetRange(0, pathComponents.Count - 1).ToArray());
        }
    }
}
#endif
