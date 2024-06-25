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
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
using System;
using UnityEngine;
using Vuplex.WebView.Internal;

namespace Vuplex.WebView {

    /// <summary>
    /// The base class for WindowsWebPlugin and MacWebPlugin.
    /// </summary>
    public class StandaloneWebPlugin : MonoBehaviour {

        public ICookieManager CookieManager { get; } = StandaloneCookieManager.Instance;

        public void ClearAllData() => StandaloneWebView.ClearAllData();

        // Deprecated
        public void CreateMaterial(Action<Material> callback) => callback(VXUtils.CreateDefaultMaterial());

        public void EnableRemoteDebugging() => StandaloneWebView.EnableRemoteDebugging(8080);

        public void SetAutoplayEnabled(bool enabled) => StandaloneWebView.SetAutoplayEnabled(enabled);

        public void SetCameraAndMicrophoneEnabled(bool enabled) => StandaloneWebView.SetCameraAndMicrophoneEnabled(enabled);

        public void SetIgnoreCertificateErrors(bool ignore)=> StandaloneWebView.SetIgnoreCertificateErrors(ignore);

        public void SetStorageEnabled(bool enabled) => StandaloneWebView.SetStorageEnabled(enabled);

        public void SetUserAgent(bool mobile) => StandaloneWebView.GloballySetUserAgent(mobile);

        public void SetUserAgent(string userAgent) => StandaloneWebView.GloballySetUserAgent(userAgent);

    #if UNITY_2020_3_OR_NEWER
        void Start() {
            // It's preferable to use Application.quitting instead
            // of OnApplicationQuit(), because the latter is called even if
            // the quit is cancelled by the application returning false from
            // Application.wantsToQuit, and the former is called only when the
            // application really quits. Application.quitting was added in 2018.1, but in
            // 2020.1 and 2020.2 it has a bug where it isn't raised when the application is
            // quit with alt+f4:
            // https://issuetracker.unity3d.com/issues/application-dot-quitting-event-is-not-raised-when-closing-build
            Application.quitting += () => StandaloneWebView.TerminateBrowserProcess();
        }
    #else
        void OnApplicationQuit() => StandaloneWebView.TerminateBrowserProcess();
    #endif
    }
}
#endif
