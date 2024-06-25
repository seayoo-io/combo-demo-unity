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
    /// Event args for IWithAuth.AuthRequested. Either Continue() or Cancel() must be called in order
    /// to resume the page.
    /// </summary>
    public class AuthRequestedEventArgs : EventArgs {

        public AuthRequestedEventArgs(string host, bool isProxy, Action<string, string> continueCallback, Action cancelCallback) {
            Host = host;
            IsProxy = isProxy;
            _continueCallback = continueCallback;
            _cancelCallback = cancelCallback;
        }

        /// <summary>
        /// The host that requested authentication.
        /// </summary>
        public readonly string Host;

        /// <summary>
        /// Indicates whether the auth request is for connecting to a proxy server.
        /// </summary>
        public readonly bool IsProxy;

        /// <summary>
        /// Declines authentication and resumes the page.
        /// </summary>
        public void Cancel() => _cancelCallback();

        /// <summary>
        /// Sends the credentials for authentication.
        /// </summary>
        public void Continue(string username, string password) => _continueCallback(username, password);

        Action _cancelCallback;
        Action<string, string> _continueCallback;
    }
}

