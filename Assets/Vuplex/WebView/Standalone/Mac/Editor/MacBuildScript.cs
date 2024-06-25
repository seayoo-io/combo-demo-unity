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
#if UNITY_STANDALONE_OSX
#pragma warning disable CS0618
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Callbacks;
using UnityEngine.Rendering;
using Vuplex.WebView.Internal;

namespace Vuplex.WebView.Editor {

    public class MacBuildScript : IPreprocessBuild {

        public int callbackOrder { get => 0; }

        public void OnPreprocessBuild(BuildTarget buildTarget, string buildPath) {

            if (buildTarget != BuildTarget.StandaloneOSX) {
                return;
            }
            #if !VUPLEX_DISABLE_GRAPHICS_API_WARNING
                var selectedGraphicsApi = PlayerSettings.GetGraphicsAPIs(buildTarget)[0];
                var error = VXUtils.GetGraphicsApiErrorMessage(selectedGraphicsApi, new GraphicsDeviceType[] { GraphicsDeviceType.Metal });
                if (error != null) {
                    throw new BuildFailedException(error);
                }
            #endif
        }

        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject) {

            if (target != BuildTarget.StandaloneOSX) {
                return;
            }
            // Delete all of the .meta files added by Unity because they cause a codesign mismatch
            // which causes app notarization to fail.
            var metaFiles = Directory.GetFiles(pathToBuiltProject, "*.meta", SearchOption.AllDirectories);
            foreach (var file in metaFiles) {
                File.Delete(file);
            }
        }
    }
}
#endif
