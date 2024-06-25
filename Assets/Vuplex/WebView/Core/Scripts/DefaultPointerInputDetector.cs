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
#pragma warning disable CS0414
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Vuplex.WebView.Internal;
#if VUPLEX_MRTK
    using Microsoft.MixedReality.Toolkit.Input;
#endif

namespace Vuplex.WebView {

    [HelpURL("https://developer.vuplex.com/webview/IPointerInputDetector")]
    public class DefaultPointerInputDetector : MonoBehaviour,
                                               IPointerInputDetector,
                                            #if VUPLEX_MRTK
                                               IMixedRealityPointerHandler,
                                               // When using MRTK, don't implement the standard Unity event interfaces because
                                               // it causes CanvasWebViewPrefab to receive click events twice (once through MRTK and
                                               // once through the standard Unity interfaces).
                                            #else
                                               IBeginDragHandler,
                                               IDragHandler,
                                               IPointerClickHandler,
                                               IPointerDownHandler,
                                               IPointerEnterHandler,
                                               IPointerExitHandler,
                                            #if UNITY_2021_1_OR_NEWER
                                               IPointerMoveHandler,
                                            #endif
                                               IPointerUpHandler,
                                            #endif
                                               IScrollHandler {

        public event EventHandler<EventArgs<Vector2>> BeganDrag;

        public event EventHandler<EventArgs<Vector2>> Dragged;

        public event EventHandler<PointerEventArgs> PointerDown;

        public event EventHandler PointerEntered;

        public event EventHandler<EventArgs<Vector2>> PointerExited;

        public event EventHandler<EventArgs<Vector2>> PointerMoved;

        public event EventHandler<PointerEventArgs> PointerUp;

        public event EventHandler<ScrolledEventArgs> Scrolled;

        public bool PointerMovedEnabled { get; set; }

        /// <see cref="IBeginDragHandler"/>
        public void OnBeginDrag(PointerEventData eventData) {

            _raiseBeganDragEvent(_convertToEventArgs(eventData));
        }

        /// <see cref="IDragHandler"/>
        public void OnDrag(PointerEventData eventData) {

            // The point is Vector3.zero when the user drags off of the screen.
            if (!_positionIsZero(eventData)) {
                _raiseDraggedEvent(_convertToEventArgs(eventData));
            }
        }

        /// <summary>
        /// VRIF requires IPointerClickHandler to be implemented in order to detect the object
        /// and invoke its OnPointerDown() and OnPointerUp() methods.
        /// </summary>
        /// <see cref="IPointerClickHandler"/>
        public void OnPointerClick(PointerEventData eventData) {}

        /// <see cref="IPointerDownHandler"/>
        public virtual void OnPointerDown(PointerEventData eventData) {

            // StandaloneInputModule and InputSystemUIInputModule both have an issue where clickCount is 1 less than what it should be in OnPointerDown:
            // https://issuetracker.unity3d.com/product/unity/issues/guid/UUM-68720
            // This issue has been observed in all of the Unity versions tested: 2020.3, 2022.3, 2023.2
            // Here's an example of what logging the clickCount in OnPointerDown and OnPointerUp for a double click looks like:
            // > OnPointerDown clickCount: 0
            // > OnPointerUp clickCount: 1
            // > OnPointerDown clickCount: 1
            // > OnPointerUp clickCount: 2
            // Originally this class tried to compensate for that by adding 1 to the OnPointerDown() clickCount value,
            // but Unity 2020.3 and 2021.3 (but not 2022.3) have another issue where after a double click, the OnPointerDown() clickCount value
            // is incorrectly set to 2 on the next click after a double click.
            // As a workaround, this class rolls its own click count detection instead.
            var now = DateTime.Now;
            var millisecondsSinceLastPointerDown = (now - _lastPointerDownDateTime).TotalMilliseconds;
            _lastPointerDownDateTime = now;
            var isDoubleClick = millisecondsSinceLastPointerDown <= 500;
            _clickCount = isDoubleClick ? _clickCount + 1 : 1;
            _raisePointerDownEvent(_convertToPointerEventArgs(eventData));
        }

        /// <see cref="IPointerEnterHandler"/>
        public void OnPointerEnter(PointerEventData eventData) {

            _isHovering = true;
            _raisePointerEnteredEvent(EventArgs.Empty);
        }

        /// <see cref="IPointerExitHandler"/>
        public void OnPointerExit(PointerEventData eventData) {

            _isHovering = false;
            // When StandaloneInputModule triggers OnPointerExit, eventData.pointerCurrentRaycast.worldPosition is usually Vector3.zero,
            // so for world space, just fallback to sending a normalized point of Vector2.zero.
            var point = _positionIsZero(eventData) ? Vector2.zero : _convertToNormalizedPoint(eventData);
            // Since this is an exit event, the coordinate can sometimes be just outside the bounds of [0, 1], so clamp it to [0, 1].
            for (var i = 0; i < 2; i++) {
                if (point[i] < 0f) {
                    point[i] = 0f;
                } else if (point[i] > 1f) {
                    point[i] = 1f;
                }
            }
            _raisePointerExitedEvent(new EventArgs<Vector2>(point));
        }

        /// <see cref="IPointerMoveHandler"/>
        public void OnPointerMove(PointerEventData eventData) {

            if (!(PointerMovedEnabled && _isHovering)) {
                return;
            }
            var point = _convertToNormalizedPoint(eventData);
            if (!(point.x >= 0f && point.y >= 0f)) {
                // This can happen while the prefab is being resized.
                return;
            }
            if (_previousPointerMovedPoint == point) {
                return;
            }
            _previousPointerMovedPoint = point;
            _raisePointerMovedEvent(new EventArgs<Vector2>(point));
        }

        /// <see cref="IPointerUpHandler"/>
        public virtual void OnPointerUp(PointerEventData eventData) {

            _raisePointerUpEvent(_convertToPointerEventArgs(eventData));
        }

        /// <see cref="IScrollHandler"/>
        public void OnScroll(PointerEventData eventData) {

            var scrollDelta = -1 * eventData.scrollDelta;
            _raiseScrolledEvent(new ScrolledEventArgs(scrollDelta, _convertToNormalizedPoint(eventData)));
        }

        int _clickCount = 1;
        DateTime _lastPointerDownDateTime = DateTime.Now;
        bool _isHovering;
        Vector2 _previousPointerMovedPoint;

        EventArgs<Vector2> _convertToEventArgs(Vector3 worldPosition) {

            var screenPoint = _convertToNormalizedPoint(worldPosition);
            return new EventArgs<Vector2>(screenPoint);
        }

        EventArgs<Vector2> _convertToEventArgs(PointerEventData pointerEventData) {

            var screenPoint = _convertToNormalizedPoint(pointerEventData);
            return new EventArgs<Vector2>(screenPoint);
        }

        protected virtual Vector2 _convertToNormalizedPoint(PointerEventData pointerEventData) {

            return _convertToNormalizedPoint(pointerEventData.pointerCurrentRaycast.worldPosition);
        }

        protected virtual Vector2 _convertToNormalizedPoint(Vector3 worldPosition) {

            // Note: transform.parent is WebViewPrefabResizer
            var localPosition = transform.parent.InverseTransformPoint(worldPosition);
            var point = new Vector2(1 - localPosition.x, -1 * localPosition.y);
            // In some cases, the point may be outside the range of [0, 1], so we need to clamp it to [0, 1]. Scenarios where that's the case:
            // - OnPointerExit()
            // - OnPointerUp(), if the mouse button is released after dragging outside of the webview.
            for (var i = 0; i < 2; i++) {
                if (point[i] < 0f) {
                    point[i] = 0f;
                } else if (point[i] > 1f) {
                    point[i] = 1f;
                }
            }
            return point;
        }

        PointerEventArgs _convertToPointerEventArgs(PointerEventData eventData) {

            return new PointerEventArgs {
                Point = _convertToNormalizedPoint(eventData),
                Button = (MouseButton)eventData.button,
                ClickCount = _clickCount
            };
        }

        PointerEventData _getLastPointerEventData() {

            var currentInputModule = UnityEngine.EventSystems.EventSystem.current == null ? null : UnityEngine.EventSystems.EventSystem.current.currentInputModule;
            // Support for input modules that derive from PointerInputModule, like StandaloneInputModule.
            var pointerInputModule = currentInputModule as PointerInputModule;
            if (pointerInputModule != null) {
                // Use reflection to get access to the protected GetPointerData()
                // method. Unity isn't going to change this API because most input modules
                // extend PointerInputModule. Note that GetPointerData() is used instead
                // of GetLastPointerEventData() because the latter doesn't work with
                // the Oculus SDK's OVRInputModule.
                var args = new object[] { PointerInputModule.kMouseLeftId, null, false };
                pointerInputModule.GetType().InvokeMember(
                    "GetPointerData",
                    BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    pointerInputModule,
                    args
                );
                // The second argument is an out param.
                var pointerEventData = args[1] as PointerEventData;
                return pointerEventData;
            }

            #if ENABLE_INPUT_SYSTEM
                // Support for the new InputSystem's InputSystemUIInputModule.
                var uiInputModule = currentInputModule as UnityEngine.InputSystem.UI.InputSystemUIInputModule;
                if (uiInputModule != null) {
                    var pointerEventData = new PointerEventData(EventSystem.current);
                    var raycastResult = uiInputModule.GetLastRaycastResult(0);
                    pointerEventData.position = raycastResult.screenPosition;
                    pointerEventData.pointerCurrentRaycast = uiInputModule.GetLastRaycastResult(0);
                    return pointerEventData;
                }
            #endif

            #if VUPLEX_XR_INTERACTION_TOOLKIT
                // Support for XR Interaction Toolkit.
                return Internal.XritPointerEventHelper.Instance.LastPointerEventData;
            #else
                return null;
            #endif
        }

        protected virtual bool _positionIsZero(PointerEventData eventData) => eventData.pointerCurrentRaycast.worldPosition == Vector3.zero;

        protected void _raiseBeganDragEvent(EventArgs<Vector2> eventArgs) => BeganDrag?.Invoke(this, eventArgs);

        protected void _raiseDraggedEvent(EventArgs<Vector2> eventArgs) => Dragged?.Invoke(this, eventArgs);

        protected void _raisePointerDownEvent(PointerEventArgs eventArgs) => PointerDown?.Invoke(this, eventArgs);

        protected void _raisePointerEnteredEvent(EventArgs eventArgs) => PointerEntered?.Invoke(this, eventArgs);

        protected void _raisePointerExitedEvent(EventArgs<Vector2> eventArgs) => PointerExited?.Invoke(this, eventArgs);

        // IPointerMoveHandler was added in Unity 2021.1, so this method attempts to manually call OnPointerMove
        // for versions of Unity older than 2021.1.
        void _processLegacyPointerMoveHandler() {

            var eventData = _getLastPointerEventData();
            if (eventData != null) {
                OnPointerMove(eventData);
            }
        }

        protected void _raisePointerMovedEvent(EventArgs<Vector2> eventArgs) => PointerMoved?.Invoke(this, eventArgs);

        protected void _raisePointerUpEvent(PointerEventArgs eventArgs) => PointerUp?.Invoke(this, eventArgs);

        protected void _raiseScrolledEvent(ScrolledEventArgs eventArgs) => Scrolled?.Invoke(this, eventArgs);

        protected virtual void Update() {

            #if !UNITY_2021_1_OR_NEWER
                _processLegacyPointerMoveHandler();
            #endif
        }

    // Code specific to Microsoft's Mixed Reality Toolkit.
    #if VUPLEX_MRTK
        bool _beganDragEmitted;

        /// <see cref="IMixedRealityPointerHandler"/>
        public void OnPointerClicked(MixedRealityPointerEventData eventData) {}

        /// <see cref="IMixedRealityPointerHandler"/>
        public void OnPointerDragged(MixedRealityPointerEventData eventData) {

            var eventArgs = _convertToEventArgs(eventData.Pointer.Result.Details.Point);
            if (_beganDragEmitted) {
                _raiseDraggedEvent(eventArgs);
            } else {
                _beganDragEmitted = true;
                _raiseBeganDragEvent(eventArgs);
            }
        }

        /// <see cref="IMixedRealityPointerHandler"/>
        public void OnPointerDown(MixedRealityPointerEventData eventData) {

            // Set IsTargetPositionLockedOnFocusLock to false, or else the Point
            // coordinates will be locked and won't change in OnPointerDragged or OnPointerUp.
            eventData.Pointer.IsTargetPositionLockedOnFocusLock = false;
            _beganDragEmitted = false;
            var screenPoint = _convertToNormalizedPoint(eventData.Pointer.Result.Details.Point);
            _raisePointerDownEvent(new PointerEventArgs { Point = screenPoint });
        }

        /// <see cref="IMixedRealityPointerHandler"/>
        public void OnPointerUp(MixedRealityPointerEventData eventData) {

            var screenPoint = _convertToNormalizedPoint(eventData.Pointer.Result.Details.Point);
            _raisePointerUpEvent(new PointerEventArgs { Point = screenPoint });
        }

        void Start() {

            WebViewLogger.Log("Just a heads-up: please ignore the warning 'BoxCollider is null...' warning from MRTK. WebViewPrefab doesn't use a BoxCollider, so it sets the bounds of NearInteractionTouchable manually, but MRTK doesn't provide a way to disable the warning.");
            // Add a NearInteractionTouchable script to allow touch interactions
            // to trigger the IMixedRealityPointerHandler methods.
            var touchable = gameObject.AddComponent<NearInteractionTouchable>();
            touchable.EventsToReceive = TouchableEventType.Pointer;
            touchable.SetBounds(Vector2.one);
            touchable.SetLocalForward(new Vector3(0, 0, -1));
        }
    #endif
    }
}
