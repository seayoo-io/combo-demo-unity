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

namespace Vuplex.WebView {

    /// <summary>
    /// An interface implemented by a webview if it supports entering credentials for [HTTP authentication](https://developer.mozilla.org/en-US/docs/Web/HTTP/Authentication)
    /// and/or connecting to a proxy server.
    /// </summary>
    public interface IWithAuth {

        /// <summary>
        /// Indicates that the browser has requested credentials from the user, either because a server requested [HTTP authentication](https://developer.mozilla.org/en-US/docs/Web/HTTP/Authentication)
        /// or because credentials are needed for connecting to a proxy server.
        /// </summary>
        /// <remarks>
        /// If no handler is attached to this event, then the host's authentication request will be ignored
        /// and the page will not be paused. If a handler is attached to this event, then the page will
        /// be paused until Continue() or Cancel() is called.
        ///
        /// You can test basic HTTP auth using [this page](https://jigsaw.w3.org/HTTP/Basic/)
        /// with the username "guest" and the password "guest".
        /// </remarks>
        /// <remarks>
        /// This event is not raised for most websites because most sites implement a custom sign-in page
        /// instead of using HTTP authentication to show the browser's built-in authentication UI.
        /// </remarks>
        /// <example>
        /// <code>
        /// await webViewPrefab.WaitUntilInitialized();
        /// var webViewWithAuth = webViewPrefab.WebView as IWithAuth;
        /// if (webViewWithAuth == null) {
        ///     Debug.Log("This 3D WebView plugin doesn't yet support IWithAuth: " + webViewPrefab.WebView.PluginType);
        ///     return;
        /// }
        /// webViewWithAuth.AuthRequested += (sender, eventArgs) => {
        ///     Debug.Log("Auth requested by " + eventArgs.Host);
        ///     eventArgs.Continue("myUsername", "myPassword");
        /// };
        /// </code>
        /// </example>
        event EventHandler<AuthRequestedEventArgs> AuthRequested;
    }

    // Renamed in v4.8.
    [Obsolete("The IWithHttpAuth interface has been renamed to IWithAuth. Please use IWithAuth instead.", true)]
    public interface IWithHttpAuth {}
}
