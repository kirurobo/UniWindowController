/*
 * UniWinCore.cs
 * 
 * Author: Kirurobo http://twitter.com/kirurobo
 * License: MIT
 */

using System;
using System.Linq;
using System.Runtime.InteropServices;
using AOT;
using Kirurobo;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kirurobo
{
    /// <summary>
    /// Windows / macOS のネイティブプラグインラッパー
    /// </summary>
    internal class UniWinCore : IDisposable
    {
        /// <summary>
        /// 透明化の方式
        /// </summary>
        public enum TransparentType
        {
            None = 0,
            Alpha = 1,
            ColorKey = 2,
        }

        #region Native functions
        protected class LibUniWinC
        {
            [UnmanagedFunctionPointer((CallingConvention.Cdecl))]
            public delegate void StringCallback([MarshalAs(UnmanagedType.LPWStr)] string returnString);

            [UnmanagedFunctionPointer((CallingConvention.Cdecl))]
            public delegate void IntCallback([MarshalAs(UnmanagedType.I4)] int value);

            
            [DllImport("LibUniWinC")]
            public static extern bool IsActive();

            [DllImport("LibUniWinC")]
            public static extern bool IsTransparent();

            [DllImport("LibUniWinC")]
            public static extern bool IsBorderless();

            [DllImport("LibUniWinC")]
            public static extern bool IsTopmost();

            [DllImport("LibUniWinC")]
            public static extern bool IsBottommost();

            [DllImport("LibUniWinC")]
            public static extern bool IsMaximized();

            [DllImport("LibUniWinC")]
            public static extern bool AttachMyWindow();

            [DllImport("LibUniWinC")]
            public static extern bool AttachMyOwnerWindow();

            [DllImport("LibUniWinC")]
            public static extern bool AttachMyActiveWindow();

            [DllImport("LibUniWinC")]
            public static extern bool DetachWindow();

            [DllImport("LibUniWinC")]
            public static extern void SetTransparent(bool bEnabled);

            [DllImport("LibUniWinC")]
            public static extern void SetBorderless(bool bEnabled);

            [DllImport("LibUniWinC")]
            public static extern void SetClickThrough(bool bEnabled);

            [DllImport("LibUniWinC")]
            public static extern void SetTopmost(bool bEnabled);

            [DllImport("LibUniWinC")]
            public static extern void SetBottommost(bool bEnabled);

            [DllImport("LibUniWinC")]
            public static extern void SetMaximized(bool bZoomed);

            [DllImport("LibUniWinC")]
            public static extern void SetPosition(float x, float y);

            [DllImport("LibUniWinC")]
            public static extern bool GetPosition(out float x, out float y);

            [DllImport("LibUniWinC")]
            public static extern void SetSize(float x, float y);

            [DllImport("LibUniWinC")]
            public static extern bool GetSize(out float x, out float y);

            [DllImport("LibUniWinC")]
            public static extern bool RegisterDropFilesCallback([MarshalAs(UnmanagedType.FunctionPtr)] StringCallback callback);

            [DllImport("LibUniWinC")]
            public static extern bool UnregisterDropFilesCallback();

            [DllImport("LibUniWinC")]
            public static extern bool RegisterMonitorChangedCallback([MarshalAs(UnmanagedType.FunctionPtr)] IntCallback callback);

            [DllImport("LibUniWinC")]
            public static extern bool UnregisterMonitorChangedCallback();

            [DllImport("LibUniWinC")]
            public static extern bool RegisterWindowStyleChangedCallback([MarshalAs(UnmanagedType.FunctionPtr)] IntCallback callback);

            [DllImport("LibUniWinC")]
            public static extern bool UnregisterWindowStyleChangedCallback();

            [DllImport("LibUniWinC")]
            public static extern bool SetAllowDrop(bool enabled);

            [DllImport("LibUniWinC")]
            public static extern int GetCurrentMonitor();

            [DllImport("LibUniWinC")]
            public static extern int GetMonitorCount();

            [DllImport("LibUniWinC")]
            public static extern bool GetMonitorRectangle(int index, out float x, out float y, out float width, out float height);

            [DllImport("LibUniWinC")]
            public static extern void SetCursorPosition(float x, float y);

            [DllImport("LibUniWinC")]
            public static extern bool GetCursorPosition(out float x, out float y);

            [DllImport("LibUniWinC")]
            public static extern void SetTransparentType(int type);

            [DllImport("LibUniWinC")]
            public static extern void SetKeyColor(uint colorref);
        }
        #endregion

        static string[] lastDroppedFiles;
        static bool wasDropped = false;
        static bool wasMonitorChanged = false;
        static bool wasWindowStyleChanged = false;

#if UNITY_EDITOR
        // 参考 http://baba-s.hatenablog.com/entry/2017/09/17/135018
        /// <summary>
        /// ゲームビューのEditorWindowを取得
        /// </summary>
        /// <returns></returns>
        public static EditorWindow GetGameView()
        {
            var assembly = typeof(EditorWindow).Assembly;
            var type = assembly.GetType("UnityEditor.GameView");
            var gameView = EditorWindow.GetWindow(type);
            return gameView;
        }
#endif

        /// <summary>
        /// ウィンドウ操作ができる状態ならtrueを返す
        /// </summary>
        /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
        public bool IsActive = false;

        /// <summary>
        /// 最前面表示になっているかどうか
        /// </summary>
        public bool IsTopmost { get { return (IsActive && _isTopmost); } }
        private bool _isTopmost = false;

        /// <summary>
        /// 常に最背面表示になっているかどうか
        /// </summary>
        public bool IsBottommost { get { return (IsActive && _isBottommost); } }
        private bool _isBottommost = false;

        /// <summary>
        /// ウィンドウ透過となっているか
        /// </summary>
        public bool IsTransparent { get { return (IsActive && _isTransparent); } }
        private bool _isTransparent = false;

        /// <summary>
        /// クリックスルー（マウス操作を受け取らない状態）となっているか
        /// </summary>
        public bool IsClickThrough { get { return (IsActive && _isClickThrough); } }
        private bool _isClickThrough = false;

        /// <summary>
        /// ウィンドウ透過方式
        /// </summary>
        private TransparentType transparentType = TransparentType.Alpha;

        /// <summary>
        /// Layered Windows で透過する色
        /// </summary>
        private Color32 ChromakeyColor = new Color32(1, 0, 1, 0);


        #region Constructor or destructor
        /// <summary>
        /// ウィンドウ制御のコンストラクタ
        /// </summary>
        public UniWinCore()
        {
            IsActive = false;
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~UniWinCore()
        {
            Dispose();
        }

        /// <summary>
        /// 終了時の処理
        /// </summary>
        public void Dispose()
        {
            // 最後にウィンドウ状態を戻すとそれが目についてしまうので、あえて戻さないことにしてみるためコメントアウト
            //DetachWindow();

            // Instead of DetachWindow()
            LibUniWinC.UnregisterDropFilesCallback();
            LibUniWinC.UnregisterMonitorChangedCallback();
            LibUniWinC.UnregisterWindowStyleChangedCallback();
        }
        #endregion

        
        #region Callbacks
        
        /// <summary>
        /// モニタまたは解像度が変化したときのコールバック
        /// この中での処理は最低限にするため、フラグを立てるのみ
        /// </summary>
        /// <param name="monitorCount"></param>
        [MonoPInvokeCallback(typeof(LibUniWinC.IntCallback))]
        private static void _monitorChangedCallback([MarshalAs(UnmanagedType.I4)] int monitorCount)
        {
            wasMonitorChanged = true;
        }

        /// <summary>
        /// ウィンドウスタイルや最大化、最小化等で呼ばれるコールバック
        /// この中での処理は最低限にするため、フラグを立てるのみ
        /// </summary>
        /// <param name="e"></param>
        [MonoPInvokeCallback(typeof(LibUniWinC.IntCallback))]
        private static void _windowStyleChangedCallback([MarshalAs(UnmanagedType.I4)] int e)
        {
            wasWindowStyleChanged = true;
        }
        
        /// <summary>
        /// ファイル、フォルダがドロップされた時に呼ばれるコールバック
        /// 文字列を配列に直すことと、フラグを立てるまで行う
        /// </summary>
        /// <param name="paths"></param>
        [MonoPInvokeCallback(typeof(LibUniWinC.StringCallback))]
        private static void _droppedFilesCallback([MarshalAs(UnmanagedType.LPWStr)] string paths)
        {
            // LF 区切りで届いた文字列を分割してパスの配列に直す
            char[] delimiters = { '\n', '\r', '\t', '\0' };
            string[] files = paths.Split(delimiters).Where(s => s != "").ToArray();

            if (files.Length > 0)
            {
                lastDroppedFiles = new string[files.Length];
                files.CopyTo(lastDroppedFiles, 0);

                wasDropped = true;
            }
        }
        
        #endregion
        
        #region Find, attach or detach 

        /// <summary>
        /// ウィンドウ状態を最初に戻して操作対象から解除
        /// </summary>
        public void DetachWindow()
        {
#if UNITY_EDITOR
            // エディタの場合、ウィンドウスタイルでは常に最前面と得られていない可能性があるため、
            //  最前面ではないのが本来と決め打ちで、デタッチ時無効化する
            EnableTopmost(false);
#endif
            LibUniWinC.DetachWindow();
        }

        /// <summary>
        /// 自分のウィンドウ（ゲームビューが独立ウィンドウならそれ）を探して操作対象とする
        /// </summary>
        /// <returns></returns>
        public bool AttachMyWindow()
        {
#if UNITY_EDITOR_WIN
            // 確実にゲームビューを得る方法がなさそうなので、フォーカスを与えて直後にアクティブなウィンドウを取得
            var gameView = GetGameView();
            if (gameView)
            {
                gameView.Focus();
                LibUniWinC.AttachMyActiveWindow();
            }
#else
        LibUniWinC.AttachMyWindow();
#endif
            // Add event handlers
            LibUniWinC.RegisterDropFilesCallback(_droppedFilesCallback);
            LibUniWinC.RegisterMonitorChangedCallback(_monitorChangedCallback);
            LibUniWinC.RegisterWindowStyleChangedCallback(_windowStyleChangedCallback);

            IsActive = LibUniWinC.IsActive();
            return IsActive;
        }

        /// <summary>
        /// 自分のプロセスで現在アクティブなウィンドウを選択
        /// エディタの場合、ウィンドウが閉じたりドッキングしたりするため、フォーカス時に呼ぶ
        /// </summary>
        /// <returns></returns>
        public bool AttachMyActiveWindow()
        {
            LibUniWinC.AttachMyActiveWindow();
            IsActive = LibUniWinC.IsActive();
            return IsActive;
        }

        #endregion

        #region About window status

        /// <summary>
        /// 透過を設定／解除
        /// </summary>
        /// <param name="isTransparent"></param>
        public void EnableTransparent(bool isTransparent)
        {
            // エディタは透過できなかったり、枠が通常と異なるのでスキップ
#if !UNITY_EDITOR
        LibUniWinC.SetTransparent(isTransparent);
        LibUniWinC.SetBorderless(isTransparent);
#endif
            this._isTransparent = isTransparent;
        }

        /// <summary>
        /// Set the window z-order (Topmost or not).
        /// </summary>
        /// <param name="isTopmost">If set to <c>true</c> is top.</param>
        public void EnableTopmost(bool isTopmost)
        {
            LibUniWinC.SetTopmost(isTopmost);
            this._isTopmost = isTopmost;
            this._isBottommost = false;    // Exclusive
        }

        /// <summary>
        /// Set the window z-order (Bottommost or not).
        /// </summary>
        /// <param name="isBottommost">If set to <c>true</c> is bottom.</param>
        public void EnableBottommost(bool isBottommost)
        {
            LibUniWinC.SetBottommost(isBottommost);
            this._isBottommost = isBottommost;
            this._isTopmost = false;    // Exclusive
        }

        /// <summary>
        /// クリックスルーを設定／解除
        /// </summary>
        /// <param name="isThrough"></param>
        public void EnableClickThrough(bool isThrough)
        {
            // エディタでクリックスルーされると操作できなくなる可能性があるため、スキップ
#if !UNITY_EDITOR
        LibUniWinC.SetClickThrough(isThrough);
#endif
            this._isClickThrough = isThrough;
        }

        /// <summary>
        /// ウィンドウを最大化（Macではズーム）する
        /// 最大化された後にサイズ変更がされることもあり、現状、確実には動作しない可能性があります
        /// </summary>
        public void SetZoomed(bool isZoomed)
        {
            LibUniWinC.SetMaximized(isZoomed);
        }

        /// <summary>
        /// ウィンドウが最大化（Macではズーム）されているかを取得
        /// 最大化された後にサイズ変更がされることもあり、現状、確実には動作しない可能性があります
        /// </summary>
        public bool GetZoomed()
        {
            return LibUniWinC.IsMaximized();
        }

        /// <summary>
        /// Set the window position.
        /// </summary>
        /// <param name="position">Position.</param>
        public void SetWindowPosition(Vector2 position)
        {
            LibUniWinC.SetPosition(position.x, position.y);
        }

        /// <summary>
        /// Get the window position.
        /// </summary>
        /// <returns>The position.</returns>
        public Vector2 GetWindowPosition()
        {
            Vector2 pos = Vector2.zero;
            LibUniWinC.GetPosition(out pos.x, out pos.y);
            return pos;
        }

        /// <summary>
        /// Set the window Size.
        /// </summary>
        /// <param name="size">Size.</param>
        public void SetWindowSize(Vector2 size)
        {
            LibUniWinC.SetSize(size.x, size.y);
        }

        /// <summary>
        /// Get the window Size.
        /// </summary>
        /// <returns>The Size.</returns>
        public Vector2 GetWindowSize()
        {
            Vector2 size = Vector2.zero;
            LibUniWinC.GetSize(out size.x, out size.y);
            return size;
        }

        #endregion

        #region About file dropping
        public void SetAllowDrop(bool enabled)
        {
            LibUniWinC.SetAllowDrop(enabled);
        }

        /// <summary>
        /// Check files dropping and unset the dropped flag
        /// </summary>
        /// <param name="files"></param>
        /// <returns>true if files were dropped</returns>
        public bool ObserveDroppedFiles(out string[] files)
        {
            files = lastDroppedFiles;

            if (!wasDropped || files == null) return false;

            wasDropped = false;
            return true;
        }

        /// <summary>
        /// Check the numbers of display or resolution changing, and unset the flag 
        /// </summary>
        /// <returns>true if changed</returns>
        public bool ObserveMonitorChanged()
        {
            if (!wasMonitorChanged) return false;

            wasMonitorChanged = false;
            return true;
        }

        /// <summary>
        /// Check window style was changed, and unset the flag 
        /// </summary>
        /// <returns>true if changed</returns>
        public bool ObserveWindowStyleChanged()
        {
            if (!wasWindowStyleChanged) return false;

            wasWindowStyleChanged = false;
            return true;
        }

        #endregion

        #region About mouse cursor
        /// <summary>
        /// Set the mouse pointer position.
        /// </summary>
        /// <param name="position">Position.</param>
        public void SetCursorPosition(Vector2 position)
        {
            LibUniWinC.SetCursorPosition(position.x, position.y);
        }

        /// <summary>
        /// Get the mouse pointer position.
        /// </summary>
        /// <returns>The position.</returns>
        public Vector2 GetCursorPosition()
        {
            Vector2 pos = Vector2.zero;
            LibUniWinC.GetCursorPosition(out pos.x, out pos.y);
            return pos;
        }

        // Not implemented
        public static bool GetCursorVisible()
        {
            return true;
        }
        #endregion

        #region for Windows only
        /// <summary>
        /// 透過方法を指定（Windowsのみ対応）
        /// </summary>
        /// <param name="type"></param>
        public void SetTransparentType(TransparentType type)
        {
            LibUniWinC.SetTransparentType((Int32)type);
            transparentType = type;
        }

        /// <summary>
        /// 単色透過の場合の透明色を指定（Windowsのみ対応）
        /// </summary>
        /// <param name="color"></param>
        public void SetKeyColor(Color32 color)
        {
            LibUniWinC.SetKeyColor((UInt32)(color.b * 0x10000 + color.g * 0x100 + color.r));
            ChromakeyColor = color;
        }
        #endregion

        #region About monitors
        /// <summary>
        /// Get the monitor index where the window is located
        /// </summary>
        /// <returns>Monitor index</returns>
        public int GetCurrentMonitor()
        {
            return LibUniWinC.GetCurrentMonitor();
        }

        /// <summary>
        /// Get the number of connected monitors
        /// </summary>
        /// <returns>Count</returns>
        public int GetMonitorCount()
        {
            return LibUniWinC.GetMonitorCount();
        }

        /// <summary>
        /// Fit the window to specified monitor
        /// </summary>
        /// <param name="monitorIndex"></param>
        /// <returns></returns>
        public bool FitToMonitor(int monitorIndex)
        {
            float dx, dy, dw, dh;
            if (LibUniWinC.GetMonitorRectangle(monitorIndex, out dx, out dy, out dw, out dh))
            {
                // 最大化状態なら一度戻す
                if (LibUniWinC.IsMaximized()) LibUniWinC.SetMaximized(false);
                
                // 指定モニタ中央座標
                float cx = dx + (dw / 2);
                float cy = dy + (dh / 2);

                // ウィンドウ中央を指定モニタ中央に移動
                float ww, wh;
                LibUniWinC.GetSize(out ww, out wh);
                float wx = cx - (ww / 2);
                float wy = cy - (wh / 2);
                LibUniWinC.SetPosition(wx, wy);
                
                // 最大化
                LibUniWinC.SetMaximized(true);

                //Debug.Log(String.Format("Monitor {4} : {0},{1} - {2},{3}", dx, dy, dw, dh, monitorIndex));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Print monitor list
        /// </summary>
        [Obsolete]
        public static void DebugMonitorInfo()
        {
            int monitors = LibUniWinC.GetMonitorCount();

            int currentMonitorIndex = LibUniWinC.GetCurrentMonitor();

            string message = "Current monitor: " + currentMonitorIndex + "\r\n";

            for (int i = 0; i < monitors; i++)
            {
                float x, y, w, h;
                bool result = LibUniWinC.GetMonitorRectangle(i, out x, out y, out w, out h);
                message += String.Format(
                    "Monitor {0}: X:{1}, Y:{2} - W:{3}, H:{4}\r\n",
                    i, x, y, w, h
                );
            }
            Debug.Log(message);
        }
        #endregion

    }
}