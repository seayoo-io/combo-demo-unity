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
using UnityEngine;
using Vuplex.WebView.Internal;

namespace Vuplex.WebView {

    partial class CanvasWebViewPrefab {

        partial void OnInit() {

            #if !UNITY_2022_1_OR_NEWER
                if (_canvas?.renderMode == RenderMode.ScreenSpaceOverlay) {
                    // Unity's issue tracker claims that this issue was fixed in 2021.2, but I observed that it still occurs with Unity 2021.3 on Apple Silicon Macs (in the Player, but not in the Editor). However, it doesn't occur in 2022.3.
                    WebViewLogger.LogWarning("Versions of Unity older than 2022.1 have a bug on macOS that sometimes prevents 3D WebView's external textures from appearing properly in a \"Screen Space - Overlay\" Canvas. To avoid the issue, it's recommended to either upgrade to Unity 2022.x or switch the Canvas's render mode to \"Screen Space - Camera\". https://issuetracker.unity3d.com/issues/external-texture-is-not-visible-in-player-slash-build-when-canvas-render-mode-is-set-to-screen-space-overlay");
                }
            #endif
        }
    }
}
#endif
