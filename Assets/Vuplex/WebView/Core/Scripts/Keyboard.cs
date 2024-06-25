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
using System;
using UnityEngine;
using UnityEngine.Serialization;
using Vuplex.WebView.Internal;

namespace Vuplex.WebView {

    /// <summary>
    /// A 3D, on-screen keyboard prefab that you can hook up to a webview for typing.
    /// You can add a Keyboard to your scene either by dragging the Keyboard.prefab file into it
    /// via the editor or by programmatically calling Keyboard.Instantiate().
    /// For use in a Canvas, please see CanvasKeyboard instead.
    /// </summary>
    /// <remarks>
    /// The keyboard UI is a React.js app that runs inside a WebViewPrefab and
    /// emits messages to C# to when keys are pressed.
    /// [The keyboard UI is open source and available on GitHub](https://github.com/vuplex/unity-keyboard).
    /// </remarks>
    /// <remarks>
    /// The keyboard supports layouts for the following languages and automatically sets the layout
    /// based on the operating system's default language: English, Spanish, French, German, Italian, Russian,
    /// Danish, Norwegian, and Swedish.
    /// </remarks>
    /// <remarks>
    /// Please note that 3D WebView's on-screen keyboard prefabs do not support Chinese, Japanese, or Korean.
    /// For those languages, please see [this article about IME support](https://support.vuplex.com/articles/chinese-japanese-and-korean)
    /// and [this section](https://support.vuplex.com/articles/chinese-japanese-and-korean#ime-unsupported)
    /// that describes how to enter characters for those languages programmatically.
    /// </remarks>///
    public class Keyboard : BaseKeyboard {

        /// <summary>
        /// Sets the keyboard's initial resolution in pixels per Unity unit.
        /// You can change the resolution to make the keyboard's content appear larger or smaller.
        /// For more information on scaling web content, see
        /// [this support article](https://support.vuplex.com/articles/how-to-scale-web-content).
        /// </summary>
        [Label("Resolution (px / Unity unit)")]
        [Tooltip("You can change this to make web content appear larger or smaller.")]
        [FormerlySerializedAs("InitialResolution")]
        public float Resolution = 1300;

        /// <summary>
        /// Gets the WebViewPrefab used for the keyboard UI, or `null` if
        /// the keyboard hasn't finished initializing yet.
        /// You can use WaitUntilInitialized() to detect when the WebViewPrefab property is ready to use.
        /// </summary>
        /// <example>
        /// <code>
        /// await keyboard.WaitUntilInitialized();
        /// keyboard.WebViewPrefab.Clicked += (sender, eventArgs) => {
        ///     Debug.Log("Keyboard was clicked");
        /// };
        /// </code>
        /// </example>
        public WebViewPrefab WebViewPrefab { get => (WebViewPrefab)_webViewPrefab; }

        /// <summary>
        /// Creates an instance using the default width and height.
        /// </summary>
        /// <example>
        /// <code>
        /// // Add a keyboard under a WebViewPrefab.
        /// var keyboard = Keyboard.Instantiate();
        /// keyboard.transform.SetParent(webViewPrefab.transform, false);
        /// keyboard.transform.localPosition = new Vector3(0, -0.31f, 0);
        /// keyboard.transform.localEulerAngles = Vector3.zero;
        /// </code>
        /// </example>
        public static Keyboard Instantiate() => Instantiate(DEFAULT_KEYBOARD_WIDTH, DEFAULT_KEYBOARD_HEIGHT);

        /// <summary>
        /// Like Instantiate(), but creates an instance using the specified width and height.
        /// </summary>
        public static Keyboard Instantiate(float width, float height) {

            var prefabPrototype = (GameObject)Resources.Load("Keyboard");
            var gameObject = (GameObject)Instantiate(prefabPrototype);
            var keyboard = gameObject.GetComponent<Keyboard>();
            keyboard.transform.localScale = new Vector3(width, height, 1);
            return keyboard;
        }

        void _initKeyboard() {

            var size = transform.localScale;
            transform.localScale = Vector3.one;
            var webViewPrefab = WebViewPrefab.Instantiate(
                size.x,
                size.y,
                _webViewOptions
            );
            _webViewPrefab = webViewPrefab;
            webViewPrefab.Resolution = Resolution;
            // Set NativeOnScreenKeyboardEnabled = true because on iOS, disabling the keyboard
            // for one webview disables it for all webviews.
            webViewPrefab.NativeOnScreenKeyboardEnabled = true;
            _webViewPrefab.transform.SetParent(transform, false);
            _setLayerRecursively(_webViewPrefab.gameObject, gameObject.layer);
            // Shift the WebViewPrefab up by half its height so that it's in the same place
            // as the palceholder.
            _webViewPrefab.transform.localPosition = Vector3.zero;
            _webViewPrefab.transform.localEulerAngles = Vector3.zero;
            _init();
            // Disable the placeholder that is used in the editor.
            var placeholder = transform.Find("Placeholder");
            if (placeholder != null) {
                placeholder.gameObject.SetActive(false);
            }
        }

        void Start() => _initKeyboard();

        const float DEFAULT_KEYBOARD_WIDTH = 0.5f;
        const float DEFAULT_KEYBOARD_HEIGHT = 0.125f;

        // Added in v1.0, removed in v3.12.
        [Obsolete("Keyboard.Init() has been removed. The Keyboard script now initializes itself automatically, so Init() no longer needs to be called.", true)]
        public void Init(float width, float height) {}

        // Added in v3.12, deprecated in v4.0.
        [Obsolete("Keyboard.InitialResolution is now deprecated. Please use Keyboard.Resolution instead.")]
        public float InitialResolution {
            get => Resolution;
            set => Resolution = value;
        }
    }
}
