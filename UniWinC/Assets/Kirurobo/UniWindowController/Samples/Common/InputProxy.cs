/**
 * A sample script of UniWindowContoller
 * 
 * Author: Kirurobo http://twitter.com/kirurobo
 * License: MIT
 */

using System;
using UnityEngine;
#if ENABLE_LEGACY_INPUT_MANAGER
// Don't use InputSystem in this script to prevent TouchPhase duplication
#elif ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Kirurobo {
    /// <summary>
    /// Input System と Input Manager の違いを吸収するためのプロキシ
    /// </summary>
    public class InputProxy
    {
        public static Vector3 mousePosition {
             get {
                return GetMousePosition();
             }
        }

        /// <summary>
        /// Input System の利用に合わせてキーアップを取得
        /// </summary>
        /// <returns></returns>
        public static bool GetKeyUp(String key)
        {
            #if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyUp(key);
            #elif ENABLE_INPUT_SYSTEM
            // 簡易的な実装。keyが１文字で、かつアルファベットか数字に対応
            // 1文字以外は escape, space のみ対応
            if (key.Length == 1) {
                Key k = Key.None;
                char c = key[0];
                if (c >= 'A' && c <= 'Z') k = (Key)Enum.ToObject(typeof(Key), (int)Key.A + (int)(c - 'A'));
                if (c >= 'a' && c <= 'z') k = (Key)Enum.ToObject(typeof(Key), (int)Key.A + (int)(c - 'a'));
                if (c >= '0' && c <= '9') k = (Key)Enum.ToObject(typeof(Key), (int)Key.Digit0 + (int)(c - '0'));
                return Keyboard.current[k].wasReleasedThisFrame;
            } else if (key == "escape") {
                return Keyboard.current.escapeKey.wasReleasedThisFrame;
            } else if (key == "space") {
                return Keyboard.current.spaceKey.wasReleasedThisFrame;
            }
            return false;
            #else
            return false;
            #endif
        }

        /// <summary>
        /// Input System の利用に合わせてマウス座標を取得
        /// </summary>
        /// <returns></returns>
        private static Vector3 GetMousePosition()
        {
            #if ENABLE_LEGACY_INPUT_MANAGER
            return Input.mousePosition;
            #elif ENABLE_INPUT_SYSTEM
            return Mouse.current.position.ReadValue();
            #else
            return Vector3.zero;
            #endif
        }

        /// <summary>
        /// マウスボタンが現在押されているか判定
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public static bool GetMouseButton(int button)
        {
            #if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButton(button);
            #elif ENABLE_INPUT_SYSTEM
            if (button == 0) return Mouse.current.leftButton.isPressed;
            if (button == 1) return Mouse.current.rightButton.isPressed;
            if (button == 2) return Mouse.current.middleButton.isPressed;
            return false;
            #else
            return false;
            #endif
        }

        /// <summary>
        /// このフレームでマウスボタンが押されたか判定
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public static bool GetMouseButtonDown(int button)
        {
            #if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButtonDown(button);
            #elif ENABLE_INPUT_SYSTEM
            if (button == 0) return Mouse.current.leftButton.wasPressedThisFrame;
            if (button == 1) return Mouse.current.rightButton.wasPressedThisFrame;
            if (button == 2) return Mouse.current.middleButton.wasPressedThisFrame;
            return false;
            #else
            return false;
            #endif
        }

        /// <summary>
        /// このフレームでマウスボタンが離されたか判定
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public static bool GetMouseButtonUp(int button) {
            #if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButtonUp(button);
            #elif ENABLE_INPUT_SYSTEM
            if (button == 0) return Mouse.current.leftButton.wasReleasedThisFrame;
            if (button == 1) return Mouse.current.rightButton.wasReleasedThisFrame;
            if (button == 2) return Mouse.current.middleButton.wasReleasedThisFrame;
            return false;
            #else
            return false;
            #endif
        }
    }
}