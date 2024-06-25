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
using UnityEngine;
using UnityEngine.XR;

namespace Vuplex.Demos {

    /// <summary>
    /// Script that makes it so that you can move the camera by holding down the control key on your
    /// keyboard and moving your mouse. When running on a device
    /// with a gyroscope, the gyroscope controls the camera rotation instead.
    /// </summary>
    class CameraRotator : MonoBehaviour {

        public GameObject InstructionMessage;
        private bool _legacyInputManagerDisabled;
        Vector2 _rotationFromMouse;

    // Disable this functionality in the WebGL player because it causes the following error in Safari in Unity 2021.3 and newer: "ReferenceError: Can't find variable: DeviceOrientationEvent".
    #if !UNITY_WEBGL
        void Start() {

            // If XR is disabled, enable the gyro so that it can be used to control the camera rotation.
            if (!XRSettings.enabled) {
                Input.gyro.enabled = true;
            }
            #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
                _legacyInputManagerDisabled = true;
            #endif

            // Show the instruction tip in the editor.
            if (Application.isEditor && InstructionMessage != null && !_legacyInputManagerDisabled) {
                InstructionMessage.SetActive(true);
            } else {
                InstructionMessage = null;
            }
        }

        /// <summary>
        /// If the device has a gyroscope, it is used to control the camera
        /// rotation. Otherwise, the user can hold down the control key on
        /// the keyboard to make the mouse control camera rotation.
        /// </summary>
        void Update() {

            // Dismiss the instruction message on the first click.
            if (InstructionMessage != null && !_legacyInputManagerDisabled && Input.GetMouseButtonDown(0)) {
                InstructionMessage.SetActive(false);
                InstructionMessage = null;
            }

            if (XRSettings.enabled) {
                // XR is enabled, so let the XR SDK control camera rotation instead.
                return;
            }

            if (SystemInfo.supportsGyroscope) {
                Camera.main.transform.Rotate(
                    -Input.gyro.rotationRateUnbiased.x,
                    -Input.gyro.rotationRateUnbiased.y,
                    Input.gyro.rotationRateUnbiased.z
                );
            } else if (!_legacyInputManagerDisabled && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) {
                float sensitivity = 10f;
                float maxYAngle = 80f;
                _rotationFromMouse.x += Input.GetAxis("Mouse X") * sensitivity;
                _rotationFromMouse.y -= Input.GetAxis("Mouse Y") * sensitivity;
                _rotationFromMouse.x = Mathf.Repeat(_rotationFromMouse.x, 360);
                _rotationFromMouse.y = Mathf.Clamp(_rotationFromMouse.y, -maxYAngle, maxYAngle);
                Camera.main.transform.rotation = Quaternion.Euler(_rotationFromMouse.y, _rotationFromMouse.x, 0);
            }
        }
    #endif
    }
}
