using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Windows / macOS のネイティブプラグインラッパー
/// </summary>
public class UniWinC : IDisposable
{

    /// <summary>
    /// 透明化の方式
    /// </summary>
    public enum TransparentType
    {
        None = 0,
        Alpha = 1,
        Mask = 2,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2
    {
        public float x;
        public float y;

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vector2 zero = new Vector2(0, 0);

        override public string ToString()
        {
            return x + ", " + y;
        }
    }

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
    /// ウィンドウ透過となっているか
    /// </summary>
    public bool IsTransparent { get { return (IsActive && _isTransparent); } }
    private bool _isTransparent = false;

    /// <summary>
    /// ウィンドウ透過方式
    /// </summary>
    public TransparentType TransparentMode = TransparentType.Alpha;

    /// <summary>
    /// マウスカーソル位置を取得・設定
    /// </summary>
    public Vector2 cursorPosition {
        get {
            Vector2 pos = Vector2.zero;
            GetCursorPosition(out pos.x, out pos.y);
            return pos;
        }
        set {
            SetCursorPosition(value.x, value.y);
        }
    }


    [DllImport("LibUniWinC.dll")]
    public static extern void AttachMyWindow();

    [DllImport("LibUniWinC.dll")]
    public static extern void SetTransparent(bool bEnabled);

    [DllImport("LibUniWinC.dll")]
    public static extern void SetBorderless(bool bEnabled);

    [DllImport("LibUniWinC.dll")]
    public static extern void SetClickThrough(bool bEnabled);

    [DllImport("LibUniWinC.dll")]
    public static extern void SetTopmost(bool bEnabled);

    [DllImport("LibUniWinC.dll")]
    public static extern bool SetPosition(float x, float y);

    [DllImport("LibUniWinC.dll")]
    public static extern bool GetPosition(out float x, out float y);

    [DllImport("LibUniWinC.dll")]
    public static extern bool SetSize(float width, float height);

    [DllImport("LibUniWinC.dll")]
    public static extern bool GetSize(out float width, out float height);

    [DllImport("LibUniWinC.dll")]
    public static extern int GetCurrentMonitor();

    [DllImport("LibUniWinC.dll")]
    public static extern int GetMonitorCount();

    [DllImport("LibUniWinC.dll")]
    public static extern bool GetMonitorRectangle(int monitorIndex, out float x, out float y, out float width, out float height);

    [DllImport("LibUniWinC.dll")]
    public static extern bool SetCursorPosition(float x, float y);

    [DllImport("LibUniWinC.dll")]
    public static extern bool GetCursorPosition(out float x, out float y);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void FileDropped([MarshalAs(UnmanagedType.LPWStr)]string files);
    [DllImport("LibUniWinC.dll")]
    public static extern bool RegisterFileDropCallback([MarshalAs(UnmanagedType.FunctionPtr)] FileDropped callback);

    [DllImport("LibUniWinC.dll")]
    public static extern bool UnregisterFileDropCallback();

    [DllImport("LibUniWinC.dll")]
    public static extern bool SetAllowDrop(bool enabled);

    // for development & testing. Windows only.
    [DllImport("LibUniWinC.dll")]
    public static extern IntPtr GetWindowHandle();

    [DllImport("LibUniWinC.dll")]
    public static extern uint GetMyProcessId();


    /// <summary>
    /// ウィンドウ制御のコンストラクタ
    /// </summary>
    public UniWinC()
    {
        AttachMyWindow();
        IsActive = true;
    }

    public void Dispose()
    {

    }

    public void Update()
    {

    }

    public void EnableTransparent(bool isTransparent)
    {
        if (!IsActive) return;
        SetTransparent(isTransparent);
        SetBorderless(isTransparent);
        this._isTransparent = isTransparent;
    }

    /// <summary>
    /// Set the window z-order (Topmost or not).
    /// </summary>
    /// <param name="isTopmost">If set to <c>true</c> is top.</param>
    public void EnableTopmost(bool isTopmost)
    {
        if (!IsActive) return;
        SetTopmost(isTopmost);
        this._isTopmost = isTopmost;
    }

    public void EnableClickThrough(bool isThrough)
    {
        if (!IsActive) return;
        SetClickThrough(isThrough);
    }

    /// <summary>
    /// Set the window position.
    /// </summary>
    /// <param name="position">Position.</param>
    public void SetWindowPosition(Vector2 position)
    {
        if (!IsActive) return;
        SetPosition(position.x, position.y);
    }

    /// <summary>
    /// Get the window position.
    /// </summary>
    /// <returns>The position.</returns>
    public Vector2 GetWindowPosition()
    {
        if (!IsActive) return Vector2.zero;
        Vector2 pos = Vector2.zero;
        GetPosition(out pos.x, out pos.y);
        return pos;
    }

    /// <summary>
    /// Get the window position.
    /// </summary>
    /// <returns>The position.</returns>
    public Vector2 GetWindowSize()
    {
        if (!IsActive) return Vector2.zero;
        Vector2 size = Vector2.zero;
        GetSize(out size.x, out size.y);
        return size;
    }

    public static bool GetCursorVisible()
    {
        return true;
    }
}