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
namespace Vuplex.WebView {

    /// <summary>
    /// Options for how drag interactions affect
    /// WebViewPrefab and CanvasWebViewPrefab.
    /// </summary>
    /// <seealso cref="WebViewPrefab.DragMode"/>
    /// <seealso cref="CanvasWebViewPrefab.DragMode"/>
    public enum DragMode {

        /// <summary>
        /// Drag interactions trigger scrolling (default). On platforms that support
        /// IWithTouch, touch events are used instead of mouse events. On platforms
        /// that don't support IWithTouch, mouse events are used, but dragging
        /// causes the page to be scrolled with IWebView.Scroll().
        /// </summary>
        DragToScroll,

        /// <summary>
        /// Drag interactions trigger dragging within the web page
        /// via mouse events (e.g. dragging to select text, drag-and-drop).
        /// </summary>
        /// <remarks>
        /// For information on the limitations of drag interactions on iOS and UWP, please see
        /// https://support.vuplex.com/articles/hover-and-drag-limitations.
        /// </remarks>
        DragWithinPage,

        /// <summary>
        /// Drag interactions have no effect.
        /// </summary>
        Disabled
    }
}
