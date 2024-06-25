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
#if (UNITY_STANDALONE_OSX && !UNITY_EDITOR) || UNITY_EDITOR_OSX
using UnityEngine;
using UnityEngine.Rendering;

namespace Vuplex.WebView {

    /// <summary>
    /// The macOS IWebView implementation.
    /// </summary>
    public class MacWebView : StandaloneWebView, IWebView {

        public WebPluginType PluginType { get; } = WebPluginType.Mac;

        public static MacWebView Instantiate() => new GameObject().AddComponent<MacWebView>();

        protected override GraphicsDeviceType[] _getSupportedGraphicsApis() => new [] { GraphicsDeviceType.Metal };

        protected override StandaloneWebView _instantiate() => Instantiate();
    }
}
#endif
