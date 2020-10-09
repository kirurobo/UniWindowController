using System;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
        ColorKey = 2,
    }
    
    protected class LibUniWinC
    {
        [DllImport("LibUniWinC")]
        public static extern bool IsActive();

        [DllImport("LibUniWinC")]
        public static extern bool IsTransparent();

        [DllImport("LibUniWinC")]
        public static extern bool IsBorderless();

        [DllImport("LibUniWinC")]
        public static extern bool IsTopmost();

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
        public static extern void SetPosition(float x, float y);

        [DllImport("LibUniWinC")]
        public static extern bool GetPosition(out float x, out float y);

        [DllImport("LibUniWinC")]
        public static extern void SetSize(float x, float y);

        [DllImport("LibUniWinC")]
        public static extern bool GetSize(out float x, out float y);

        [DllImport("LibUniWinC")]
        public static extern void SetTransparentType(Int32 type);

        [DllImport("LibUniWinC")]
        public static extern void SetKeyColor(UInt32 colorref);
    }


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



    /// <summary>
    /// ウィンドウ制御のコンストラクタ
    /// </summary>
    public UniWinC()
    {
        IsActive = false;
    }

    /// <summary>
    /// デストラクタ
    /// </summary>
    ~UniWinC()
    {
        Dispose();
    }

    public void Dispose()
    {
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



    // Not implemented

    public static bool GetCursorVisible()
    {
        return true;
    }
}