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
// This can't be imported as just OptionalDependencyAttribute because
// it would clash with other packages that define that attribute, like
// XR Interaction Toolkit.
#if UNITY_EDITOR
using VuplexOptionalDependencyAttribute = Vuplex.WebView.Editor.OptionalDependencyAttribute;

// Detect if specific packages are installed and, if so,
// define scripting symbols so that 3D WebView can handle them.
[assembly: VuplexOptionalDependency("Unity.XR.Oculus.OculusLoader", "VUPLEX_OCULUS")]
[assembly: VuplexOptionalDependency("OVRProjectConfig", "VUPLEX_OCULUS_PROJECT_CONFIG")]
[assembly: VuplexOptionalDependency("Unity.PolySpatial.VolumeCamera", "VUPLEX_POLYSPATIAL")]
[assembly: VuplexOptionalDependency("UnityEngine.XR.OpenXR.Features.MetaQuestSupport.MetaQuestFeature", "VUPLEX_OPENXR_META_QUEST")]
[assembly: VuplexOptionalDependency("Microsoft.MixedReality.Toolkit.MixedRealityToolkit", "VUPLEX_MRTK")]
[assembly: VuplexOptionalDependency("Vuplex.WebView.StandaloneWebPlugin", "VUPLEX_STANDALONE")]
// It's possible to download and use the visionOS build support without a Unity Pro license
// (e.g. to run a build server that builds addressables for a game). The com.unity.xr.visionos package is what's
// gated behind a Unity Pro license. So, VUPLEX_VISIONOS_XR is used to detect the presence of com.unity.xr.visionos.
[assembly: VuplexOptionalDependency("UnityEngine.XR.VisionOS.VisionOS", "VUPLEX_VISIONOS_XR")]
[assembly: VuplexOptionalDependency("UnityEngine.XR.Interaction.Toolkit.UI.XRUIInputModule", "VUPLEX_XR_INTERACTION_TOOLKIT")]
#endif
