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
using System.Threading.Tasks;

namespace Vuplex.WebView {

    /// <summary>
    /// The ICookieManager implementation for Windows and macOS.
    /// </summary>
    public class StandaloneCookieManager : ICookieManager {

        public static StandaloneCookieManager Instance {
            get {
                if (_instance == null) {
                    _instance = new StandaloneCookieManager();
                }
                return _instance;
            }
        }

        public Task<bool> DeleteCookies(string url, string cookieName = null) => StandaloneWebView.DeleteCookies(url, cookieName);

        public Task<Cookie[]> GetCookies(string url, string cookieName = null) => StandaloneWebView.GetCookies(url, cookieName);

        public Task<bool> SetCookie(Cookie cookie) => StandaloneWebView.SetCookie(cookie);

        static StandaloneCookieManager _instance;
    }
}
#endif
