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

namespace Vuplex.WebView {

    /// <summary>
    ///  Margin type enum for StandalonePdfOptions.MarginType.
    /// </summary>
    public enum StandalonePdfMarginType {

        /// <summary>
        /// Default margins of 1cm (~0.4 inches).
        /// </summary>
        Default,

        /// <summary>
        /// No margins.
        /// </summary>
        None,

        /// <summary>
        /// Custom margins specified with StandalonePdfOptions.MarginTop, MarginRight, MarginBottom, and MarginLeft.
        /// </summary>
        Custom
    }

    /// <summary>
    ///  Optional PDF formatting settings that can be passed to StandaloneWebView.CreatePdf().
    /// </summary>
    [Serializable]
    public class StandalonePdfOptions {

        /// <summary>
        /// Set to true for landscape mode or false for portrait mode. The default is false.
        /// </summary>
        public bool Landscape;

        /// <summary>
        /// The bottom margin in inches. Only used if MarginType is set to StandalonePdfMarginType.Custom.
        /// </summary>
        public float MarginBottom;

        /// <summary>
        /// The left margin in inches. Only used if MarginType is set to StandalonePdfMarginType.Custom.
        /// </summary>
        public float MarginLeft;

        /// <summary>
        /// The right margin in inches. Only used if MarginType is set to StandalonePdfMarginType.Custom.
        /// </summary>
        public float MarginRight;

        /// <summary>
        /// The top margin in inches. Only used if MarginType is set to StandalonePdfMarginType.Custom.
        /// </summary>
        public float MarginTop;

        /// <summary>
        /// The margin type. The default is StandalonePdfMarginType.Default.
        /// </summary>
        public StandalonePdfMarginType MarginType = StandalonePdfMarginType.Default;

        /// <summary>
        /// Paper ranges to print, one based, e.g., "1-5, 8, 11-13". Pages are printed
        /// in the document order, not in the order specified, and no more than once.
        /// Defaults to empty string, which implies the entire document is printed.
        /// The page numbers are quietly capped to actual page count of the document,
        /// and ranges beyond the end of the document are ignored. If this results in
        /// no pages to print, an error is reported. It is an error to specify a range
        /// with start greater than end.
        /// </summary>
        public string PageRanges = "";

        /// <summary>
        /// Output paper height in inches. If either PaperWidth or PaperHeight is less than or
        /// equal to zero, then the default paper size (letter, 8.5 x 11 inches) will
        /// be used.
        /// </summary>
        public float PaperHeight;

        /// <summary>
        /// Output paper width in inches. If either PaperWidth or PaperHeight is less than or
        /// equal to zero, then the default paper size (letter, 8.5 x 11 inches) will
        /// be used.
        /// </summary>
        public float PaperWidth;

        /// <summary>
        /// Set to true to prefer page size as defined by CSS. Defaults to false,
        /// in which case the content will be scaled to fit the paper size.
        /// </summary>
        public bool PreferCssPageSize;

        /// <summary>
        /// Set to true to print background graphics. The default is false.
        /// </summary>
        public bool PrintBackground;

        /// <summary>
        /// The percentage to scale the PDF by before printing (e.g. .5 is 50%).
        /// If this value is less than or equal to zero the default value of 1.0
        /// will be used.
        /// </summary>
        public float Scale;

        public string ToJson() => JsonUtility.ToJson(this);

        public override string ToString() => ToJson();
    }
}
