/*
 * UniWindowDragMove.cs
 * 
 * Author: Kirurobo http://twitter.com/kirurobo
 * License: MIT
 */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_LEGACY_INPUT_MANAGER
#elif ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace  Kirurobo
{
    public class UniWindowMoveHandle : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerUpHandler
    {
        private UniWindowController _uniwinc;

        /// <summary>
        /// ウィンドウが最大化されているときは移動を無効にするか
        /// </summary>
        [Tooltip("Disable drag-move when the window is zoomed (maximized).")]
        public bool disableOnZoomed = true;

        /// <summary>
        /// ドラッグ中なら true
        /// </summary>
        public bool IsDragging
        {
            get { return _isDragging; }
        }
        private bool _isDragging = false;

        /// <summary>
        /// ドラッグを行なうか否か
        /// </summary>
        private bool IsEnabled
        {
            get { return enabled && (!disableOnZoomed || !IsZoomed); }
        }

        /// <summary>
        /// モニタにフィットさせるか、最大化している
        /// </summary>
        private bool IsZoomed
        {
            get { return (_uniwinc && (_uniwinc.shouldFitMonitor || _uniwinc.isZoomed)); }
        }

        /// <summary>
        /// ドラッグ前には自動ヒットテストが有効だったかを記憶
        /// </summary>
        private bool _isHitTestEnabled;
        
        /// <summary>
        /// ドラッグ開始時のウィンドウ内座標[px]
        /// </summary>
        private Vector2 _dragStartedPosition;
        
        // Start is called before the first frame update
        void Start()
        {
            // シーン中の UniWindowController を取得
            _uniwinc = GameObject.FindAnyObjectByType<UniWindowController>();
            if (_uniwinc) _isHitTestEnabled = _uniwinc.isHitTestEnabled;

            //// ↓なくても良さそうなので勝手に変更しないようコメントアウト
            //Input.simulateMouseWithTouches = false;
        }

        /// <summary>
        /// ドラッグ開始時の処理
        /// </summary>
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsEnabled)
            {
                return;
            }

            // マウス左ボタンでのみドラッグ
            if (eventData.button != PointerEventData.InputButton.Left) return;
            
            // Macだと挙動を変える
            //  実際はRetinaサポートが有効のときだけだが、
            //  eventData.position の系と、ウィンドウ座標系でスケールが一致しなくなってしまう
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            _dragStartedPosition = _uniwinc.windowPosition - _uniwinc.cursorPosition;
#else
            _dragStartedPosition = eventData.position;
#endif
        
            // _isDragging が false ならこれからドラッグ開始と判断
            if (!_isDragging)
            {
                // ドラッグ中はヒットテストを無効にする
                _isHitTestEnabled = _uniwinc.isHitTestEnabled;
                _uniwinc.isHitTestEnabled = false;
                _uniwinc.isClickThrough = false;
            }
            
            _isDragging = true;
        }

        /// <summary>
        /// ドラッグ終了時の処理
        /// </summary>
        public void OnEndDrag(PointerEventData eventData)
        {
            EndDragging();
        }

        /// <summary>
        /// マウスが上がった際もドラッグ終了とみなす
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerUp(PointerEventData eventData)
        {
            EndDragging();
        }

        /// <summary>
        /// ドラッグ終了とする
        /// </summary>
        private void EndDragging()
        {
            if (_isDragging)
            {
                _uniwinc.isHitTestEnabled = _isHitTestEnabled; 
            }
            _isDragging = false;
        }
        
        /// <summary>
        /// 最大化時以外なら、マウスドラッグによってウィンドウを移動
        /// </summary>
        public void OnDrag(PointerEventData eventData)
        {
            if (!_uniwinc || !_isDragging) return;

            // ドラッグでの移動が無効化されていた場合はドラッグ終了
            if (!IsEnabled)
            {
                EndDragging();
                return;
            }

            // // マウス左ボタンが押されていなければドラッグ終了
            // if (eventData.button != PointerEventData.InputButton.Left) return;

            // [Shift]、[Ctrl]、[Alt]、[Command] キーが押されている間はドラッグとして扱わない
            var modifiers = UniWindowController.GetModifierKeys();
            if (modifiers != UniWindowController.ModifierKey.None) return;

            // マウスボタンが離されていればドラッグ終了
            var buttons = UniWindowController.GetMouseButtons();
            if ((buttons & UniWindowController.MouseButton.Left) == UniWindowController.MouseButton.None) {
                EndDragging();
                return;
            }
// #if ENABLE_LEGACY_INPUT_MANAGER
//             // Macの場合、マルチモニター間を移動するとEventSystemのOnEndDragが正しく呼ばれないため、マウスボタンを常に監視
//             if (!Input.Mouse.Button(0).IsPressed) {
//                 EndDragging();
//                 return;
//             }
//             if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)
//                 || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)
//                 || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) return;
// #elif ENABLE_INPUT_SYSTEM
//             // Macの場合、マルチモニター間を移動するとEventSystemのOnEndDragが正しく呼ばれないため、マウスボタンを常に監視
//             if (!Mouse.current.leftButton.isPressed) {
//                 EndDragging();
//                 return;
//             }
//             if (Keyboard.current[Key.LeftShift].isPressed || Keyboard.current[Key.RightShift].isPressed
//                 || Keyboard.current[Key.LeftCtrl].isPressed || Keyboard.current[Key.RightCtrl].isPressed
//                 || Keyboard.current[Key.LeftAlt].isPressed || Keyboard.current[Key.RightAlt].isPressed) return;
// #endif

            // フルスクリーンならウィンドウ移動は行わない
            //  エディタだと true になってしまうようなので、エディタ以外でのみ確認
#if !UNITY_EDITOR
            if (Screen.fullScreen)
            {
                EndDragging();
                return;
            }
#endif

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            // Macの場合、ネイティブプラグインでのカーソル位置取得・設定
            _uniwinc.windowPosition = _uniwinc.cursorPosition + _dragStartedPosition;
            //Debug.Log("Drag start: " + _dragStartedPosition);
#else
            // Windowsなら、タッチ操作も対応させるために eventData.position を使用する
            // スクリーンポジションが開始時の位置と一致させる分だけウィンドウを移動
            _uniwinc.windowPosition += eventData.position - _dragStartedPosition;
#endif
        }
    }
}
