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
using System.IO;
using UnityEngine;
using UnityEditor;
using Vuplex.WebView.Internal;

namespace Vuplex.WebView.Editor {

    /// <summary>
    /// Implements the editor window for enabling proprietary video codecs:
    /// https://support.vuplex.com/articles/how-to-enable-proprietary-video-codecs
    /// </summary>
    public class StandaloneVideoCodecsWindow : EditorWindow {

        [MenuItem("Vuplex/Enable Proprietary Video Codecs")]
        public static void ShowWindow() {

            // Use GetWindow() instead of GetWindowWithRect() because the latter
            // removes the ability for the user to resize the window if needed
            // (e.g. if their system is configured to use a larger font size).
            var window = EditorWindow.GetWindow(
                typeof(StandaloneVideoCodecsWindow),
                true,
                "Enable Proprietary Video Codecs | Vuplex"
            );
            window.minSize = new Vector2(500, 500);
        }

        bool _checkboxEnabled;
        bool _codecsEnabled;
        string _errorMessage;
        const string DESCRIPTION_TEXT = @"
3D WebView for Windows and macOS uses Chromium as its browser engine. <b>By default, Chromium doesn't include support for the proprietary H.264 video codec that is commonly used for MP4 and video streaming.</b> This is because companies must first sign an AVC/H.264 license agreement with Via LA (formerly known as MPEG-LA) before they can legally distribute end-user software containing the H.264 codec. By default, 3D WebView for Windows and macOS uses a build of Chromium without proprietary video codecs so that customers can use 3D WebView in their applications without needing to sign a license agreement. That means that by default, 3D WebView supports open video codecs like WebM and Ogg, but MP4 and video streaming generally do not work because they rely on H.264.

However, 3D WebView also includes a second, optional build of Chromium with proprietary codecs enabled, which companies can opt into using if they have signed an Via LA AVC/H.264 license agreement. Vuplex, Inc has signed such an agreement in order to be able to legally distribute this version of Chromium. <b>If your company has signed an AVC/H.264 license agreement with Via LA, then you can use the button below to opt into using the version of Chromium with proprietary codecs enabled.</b> If your company has not signed a license agreement with Via LA, then you are strictly prohibited from redistributing the version of Chromium with proprietary codecs enabled, but you may use it locally in the editor for personal use.
";
        const string SUPPORT_LINK_URL = "https://support.vuplex.com/articles/how-to-enable-proprietary-video-codecs";
        const string TOGGLE_TEXT = "<b>I acknowledge that I am prohibited from redistributing the version of Chromium with proprietary video codecs enabled unless I have signed an AVC/H.264 license agreement with Via LA.</b>";

        void _enableProprietaryVideoCodecs() {
            try {
                _enableProprietaryVideoCodecsForWindows();
            } catch (Exception e) {
                var message = "An error occurred while enabling proprietary video codecs for Windows: " + e.ToString();
                // If running on Windows, show the error message and stop the process.
                // If running on Mac, log the error to the console as a warning but continue to enabling Mac.
                if (Application.platform == RuntimePlatform.WindowsEditor) {
                    WebViewLogger.LogError(message);
                    _errorMessage = message;
                    return;
                } else {
                    WebViewLogger.LogWarning(message);
                }
            }
            try {
                _enableProprietaryVideoCodecsForMac();
            } catch (Exception e) {
                // If running on Mac, show the error message and stop the process.
                // If running on Windows, log the error to the console as a warning but show the process has completed.
                if (Application.platform == RuntimePlatform.OSXEditor) {
                    var message = "An error occurred while enabling proprietary video codecs for macOS: " + e.ToString();
                    WebViewLogger.LogError(message);
                    _errorMessage = message;
                    return;
                } else {
                    WebViewLogger.LogWarning("An error occurred while enabling proprietary video codecs for macOS. This can happen if the file path is over 255 characters in length. Error: " + e.ToString());
                }
            }
            _codecsEnabled = true;
        }

        void _enableProprietaryVideoCodecsForMac() {

            var sourcePath = EditorUtils.FindDirectory(
                Path.Combine(Application.dataPath, "Vuplex/WebView/Standalone/Mac/Plugins/VuplexWebViewMac_with_codecs.bundle")
            );
            var destinationPath = EditorUtils.FindDirectory(
                Path.Combine(Application.dataPath, "Vuplex/WebView/Standalone/Mac/Plugins/VuplexWebViewMac.bundle")
            );
            EditorUtils.CopyAndReplaceDirectory(sourcePath, destinationPath, false);
        }

        void _enableProprietaryVideoCodecsForWindows() {

            var sourcePath = EditorUtils.FindFile(
                Path.Combine(Application.dataPath, "Vuplex/WebView/Standalone/Windows/Plugins/libcef_with_codecs.dll")
            );
            var destinationPath = EditorUtils.FindFile(
                Path.Combine(Application.dataPath, "Vuplex/WebView/Standalone/Windows/Plugins/VuplexWebViewChromium/libcef.dll")
            );
            File.Copy(sourcePath, destinationPath, true);
        }

        void OnGUI() {

            var textColor = EditorGUIUtility.isProSkin ? "white" : "black";

            GUILayout.Box(
                EditorUtils.TextWithColor(DESCRIPTION_TEXT, textColor),
                new GUIStyle {
                    wordWrap = true,
                    richText = true,
                    padding = new RectOffset {
                        top = 20,
                        right = 20,
                        left = 20
                    }
                },
                null
            );

            // Add flexible space to center the button.
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            var learnMoreLinkClicked = GUILayout.Button(
                EditorUtils.TextWithColor("Learn more", EditorUtils.GetLinkColor()),
                 new GUIStyle {
                    richText = true,
                    padding = new RectOffset {
                        top = 10,
                        bottom = 15
                    }
                },
                new GUILayoutOption[] { GUILayout.ExpandWidth(false) }
            );
            var linkRect = GUILayoutUtility.GetLastRect();
            EditorGUIUtility.AddCursorRect(linkRect, MouseCursor.Link);
            // Unity's editor GUI doesn't support underlines, so fake it.
            GUI.Label(linkRect, EditorUtils.TextWithColor("____________", EditorUtils.GetLinkColor()), new GUIStyle {
                richText = true,
                padding = new RectOffset {
                    top = 12,
                    bottom = 15
                }
            });
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (learnMoreLinkClicked) {
                Application.OpenURL(SUPPORT_LINK_URL);
            }

            if (_errorMessage != null) {
                GUILayout.Label(
                    EditorUtils.TextWithColor(_errorMessage, "red"),
                    new GUIStyle {
                        wordWrap = true,
                        richText = true,
                        padding = new RectOffset {
                            right = 20,
                            left = 20
                        }
                    }
                );
            } else if (_codecsEnabled) {
                GUILayout.Label(
                    $"<color=green>✔</color> <color={textColor}>Proprietary video codecs enabled</color>",
                    new GUIStyle {
                        wordWrap = true,
                        richText = true,
                        alignment = TextAnchor.MiddleCenter,
                        padding = new RectOffset {
                            top = 20,
                            right = 20,
                            left = 20
                        },
                        fontSize = 14
                    }
                );
            } else {

                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                _checkboxEnabled = GUILayout.Toggle(
                    _checkboxEnabled,
                    ""
                );
                GUILayout.Label(
                    EditorUtils.TextWithColor(TOGGLE_TEXT, textColor),
                    new GUIStyle {
                        wordWrap = true,
                        padding = new RectOffset {
                            right = 20,
                        },
                        richText = true
                    }
                );
                GUILayout.EndHorizontal();
                GUILayout.Space(20);

                // Add flexible space to center the button.
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUI.enabled = _checkboxEnabled;
                var enableCodecsButtonClicked = GUILayout.Button(
                    "Enable Proprietary Video Codecs",
                    new GUILayoutOption[] { GUILayout.ExpandWidth(false) }
                );
                GUI.enabled = true;
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                if (enableCodecsButtonClicked) {
                    _enableProprietaryVideoCodecs();
                }
            }
        }
    }
}
