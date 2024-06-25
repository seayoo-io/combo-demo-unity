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
#if UNITY_EDITOR_OSX
using System.Diagnostics;
using System.IO;
using UnityEditor;

namespace Vuplex.WebView.Editor {

    /// <summary>
    /// Mac editor script that modifies the VuplexWebViewMac
    /// native plugin downloaded from the Asset Store to allow it to run.
    /// </summary>
    [InitializeOnLoad]
    class MacEditorScript {

        static MacEditorScript() {

            var macPluginPath = EditorUtils.FindDirectory("Assets/Vuplex/WebView/Standalone/Mac/Plugins/VuplexWebViewMac.bundle");

            // This removes the com.apple.quarantine attribute that
            // macOS adds to the VuplexWebViewMac.bundle plugin when it's downloaded
            // from the internet, which prevents the Unity editor from loading it.
            //
            // This is currently needed in order for the Unity
            // editor to load 3rd party plugins, because the Unity editor app doesn't
            // yet include the com.apple.security.cs.disable-library-validation
            // entitlement that's required to be able to load 3rd party bundles
            // with macOS 10.15 Catalina's hardened runtime. Without the
            // com.apple.security.cs.disable-library-validation entitlement,
            // the hardened runtime's library validation prevents an app from
            // loading libraries unless they’re either signed by Apple or signed with the
            // same team ID as the app (i.e. Unity's team ID in this case).
            //
            // References:
            // • https://developer.apple.com/documentation/bundleresources/entitlements/com_apple_security_cs_disable-library-validation
            // • https://bugzilla.mozilla.org/show_bug.cgi?id=1570840
            // • https://bugzilla.mozilla.org/show_bug.cgi?id=1470597
            _executeBashCommand("xattr -d com.apple.quarantine \"" + macPluginPath + "\"");

            // When Unity's Asset Store tools compress and decompress the asset, the executable permission
            // gets stripped off of the plugin's executables. So, we add the executable permission back
            // on here using chmod.
            var binaryRelativePaths = new string[] {
                "Contents/Frameworks/Vuplex WebView.app/Contents/MacOS/Vuplex WebView",
                "Contents/Frameworks/Vuplex WebView.app/Contents/Frameworks/Vuplex WebView Helper.app/Contents/MacOS/Vuplex WebView Helper",
                "Contents/Frameworks/Vuplex WebView.app/Contents/Frameworks/Vuplex WebView Helper (GPU).app/Contents/MacOS/Vuplex WebView Helper (GPU)",
                "Contents/Frameworks/Vuplex WebView.app/Contents/Frameworks/Vuplex WebView Helper (Plugin).app/Contents/MacOS/Vuplex WebView Helper (Plugin)",
                "Contents/Frameworks/Vuplex WebView.app/Contents/Frameworks/Vuplex WebView Helper (Renderer).app/Contents/MacOS/Vuplex WebView Helper (Renderer)"
            };
            foreach (var relativePath in binaryRelativePaths) {
                var path = Path.Combine(macPluginPath, relativePath);
                _chmod("755 \"" + path + "\"");
            }
        }

        static string _executeBashCommand(string command) {

            // Escape quotes
            command = command.Replace("\"","\"\"");
            var proc = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = "/bin/bash",
                    Arguments = "-c \""+ command + "\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            proc.WaitForExit();
            return proc.StandardOutput.ReadToEnd();
        }

        static string _chmod(string arguments) {

            var proc = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = "/bin/chmod",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            proc.WaitForExit();
            return proc.StandardOutput.ReadToEnd();
        }
    }
}
#endif
