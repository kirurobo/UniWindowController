/*
 * UniWindowController.cs
 * 
 * Author: Kirurobo http://twitter.com/kirurobo
 * License: MIT
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
using UnityEngine.Events;
#endif

namespace Kirurobo
{
    /// @cond DOXYGEN_SHOW_INTERNAL_CLASSES

    /// <summary>
    /// Set editable a bool property
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class EditablePropertyAttribute : UnityEngine.PropertyAttribute { }

    /// <summary>
    /// Set an attribute as readonly
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ReadOnlyAttribute : UnityEngine.PropertyAttribute { }

    /// @endcond


    /// <summary>
    /// Unified window controller for Windows / Mac
    /// </summary>
    public class UniWindowController : MonoBehaviour
    {
        /// <summary>
        /// The same as UniWinCore.TransparentType
        /// </summary>
        public enum TransparentType : int
        {
            None = 0,
            Alpha = 1,
            ColorKey = 2,
        }

        /// <summary>
        /// Scecifies method to hit-test (i.e., switching click-through)
        /// </summary>
        public enum HitTestType : int
        {
            None = 0,
            Opacity = 1,
            Raycast = 2,
        }

        /// <summary>
        /// Identifies the type of <see cref="OnStateChanged">OnStateChanged</see> event when it occurs
        /// </summary>
        [Flags]
        public enum WindowStateEventType : int
        {
            None = 0,
            StyleChanged = 1,
            Resized = 2,

            // 以降は仕様変更もありえる
            TopMostEnabled = 16 + 1 + 8,
            TopMostDisabled = 16 + 1,
            BottomMostEnabled = 32 + 1 + 8,
            BottomMostDisabled = 32 + 1,
            WallpaperModeEnabled = 64 + 1 + 8,
            WallpaperModeDisabled = 64 + 1,
        };

        /// <summary>
        /// Get the current instance of UniWindowController
        /// </summary>
        public static UniWindowController current => _current ? _current : FindOrCreateInstance();
        private static UniWindowController _current;

        /// <summary>
        /// Low level class
        /// </summary>
        private UniWinCore _uniWinCore = null;

        /// <summary>
        /// Is this window receives mouse events
        /// </summary>
        public bool isClickThrough
        {
            get { return _isClickThrough; }
            set { SetClickThrough(value); }
        }
        private bool _isClickThrough = false;

        /// <summary>
        /// Is this window transparent
        /// </summary>
        public bool isTransparent
        {
            get { return _isTransparent; }
            set { SetTransparent(value); }
        }
        [SerializeField, EditableProperty, Tooltip("Check to set transparent on startup")]
        private bool _isTransparent = false;

        /// <summary>
        /// Window alpha (0.0 to 1.0)
        /// </summary>
        public float alphaValue
        {
            get { return _alphaValue; }
            set { SetAlphaValue(value); }
        }
        [SerializeField, EditableProperty, Tooltip("Window alpha"), Range(0f, 1f)]
        private float _alphaValue = 1.0f;

        /// <summary>
        /// Is this window topmost
        /// </summary>
        public bool isTopmost
        {
            get { return ((_uniWinCore == null) ? _isTopmost : _isTopmost = _uniWinCore.IsTopmost); }
            set { SetTopmost(value); }
        }
        [SerializeField, EditableProperty, Tooltip("Check to set topmost on startup")]
        private bool _isTopmost = false;

        /// <summary>
        /// Is this window bottommost
        /// </summary>
        public bool isBottommost
        {
            get { return ((_uniWinCore == null) ? _isBottommost : _isBottommost = _uniWinCore.IsBottommost); }
            set { SetBottommost(value); }
        }
        [SerializeField, EditableProperty, Tooltip("Check to set bottommost on startup")]
        private bool _isBottommost = false;

        /// <summary>
        /// Is this window minimized
        /// </summary>
        public bool isZoomed
        {
            get { return ((_uniWinCore == null) ? _isZoomed : _isZoomed = _uniWinCore.GetZoomed()); }
            set { SetZoomed(value); }
        }
        [SerializeField, EditableProperty, Tooltip("Check to set zoomed on startup")]
        private bool _isZoomed = false;

        /// <summary>
        /// This window will fit to the monitor or not
        /// </summary>
        public bool shouldFitMonitor
        {
            get { return _shouldFitMonitor; }
            set { FitToMonitor(value, _monitorToFit); }
        }
        [SerializeField, EditableProperty, Tooltip("Check to fit the window to the monitor")]
        private bool _shouldFitMonitor = false;

        /// <summary>
        /// Target monitor index to fit the window (0, 1, ...)
        /// </summary>
        public int monitorToFit
        {
            get { return _monitorToFit; }
            set { FitToMonitor(_shouldFitMonitor, value); }
        }
        private int _monitorToFit = 0;

        /// <summary>
        /// Enable / disable accepting file drop
        /// </summary>
        public bool allowDropFiles
        {
            get { return _allowDropFiles; }
            set { SetAllowDrop(value); }
        }
        [SerializeField, EditableProperty, Tooltip("Enable file or folder dropping")]
        private bool _allowDropFiles = false;

        /// <summary>
        /// クリックスルー自動判定を行うか
        /// 行なわない場合は isClickThrough を自分で変更可
        /// </summary>
        public bool isHitTestEnabled = true;

        /// <summary>
        /// クリックスルー自動判定の方法
        /// </summary>
        [Tooltip("Select the method")]
        public HitTestType hitTestType = HitTestType.Opacity;

        /// <summary>
        /// クリックスルー判定方法が不透明度の場合に使うしきい値
        /// カーソル下のアルファがこの値以上ならヒットとなる
        /// </summary>
        [Tooltip("Available on the hit test type is Opacity"), RangeAttribute(0f, 1f)]
        public float opacityThreshold = 0.1f;

        /// <summary>
        /// クリックスルー判定方法が raycast の場合の最遠値
        /// </summary>
        private float raycastMaxDepth = 100.0f;

        /// <summary>
        /// trueにしておくと、ウィンドウ透過時にカメラ背景を単色の黒透明に自動で変更します
        /// </summary>
        [Header("Advanced settings")]
        [Tooltip("Change camera background when the window is transparent")]
        public bool autoSwitchCameraBackground = true;

        /// <summary>
        /// trueにしておくと、起動時にフルスクリーンだった場合は強制的に解除します
        ///
        /// 起動時のダイアログでフルスクリーンにしてしまった場合でもウィンドウモードにするためのものです
        /// 起動時のみ働きます
        /// Macの場合、フルスクリーン状態を強制解除しても別画面になったままであまり有効ではなさそうです
        /// </summary>
        [Tooltip("Force windowed on startup")]
        public bool forceWindowed = false;

        /// <summary>
        /// カメラのインスタンス
        /// </summary>
        [Tooltip("Main camera is used if None")]
        public Camera currentCamera;

        /// <summary>
        /// 透過方式の指定
        /// </summary>
        [Header("For Windows only")]
        [Tooltip("Select the method. *Only available on Windows")]
        public TransparentType transparentType = TransparentType.Alpha;

        /// <summary>
        /// Key color used when the transparent-type is ColorKey
        /// </summary>
        [Tooltip("Will be used the next time the window becomes transparent")]
        public Color32 keyColor = new Color32(0x01, 0x00, 0x01, 0x00);
        
        /// <summary>
        /// Is the mouse pointer on an opaque pixel or an object
        /// </summary>
        [Header("State")]
        [SerializeField, ReadOnly, Tooltip("Is the mouse pointer on an opaque pixel? (Read only)")]
        private bool onObject = true;
        
        /// <summary>
        /// Pixel color under the mouse pointer. (Read only)
        /// </summary>
        [SerializeField, ReadOnly, Tooltip("Pixel color under the mouse pointer. (Read only)")]
        public Color pickedColor;
        
        /// <summary>
        /// ウィンドウ座標を取得・設定
        /// </summary>
        public Vector2 windowPosition
        {
            get { return (_uniWinCore != null ? _uniWinCore.GetWindowPosition() : Vector2.zero); }
            set { _uniWinCore?.SetWindowPosition(value); }
        }

        /// <summary>
        /// ウィンドウ座標を取得・設定
        /// </summary>
        public Vector2 windowSize
        {
            get { return (_uniWinCore != null ? _uniWinCore.GetWindowSize() : Vector2.zero); }
            set { _uniWinCore?.SetWindowSize(value); }
        }

        /// <summary>
        /// クライアント領域のサイズを取得
        /// </summary>
        public Vector2 clientSize
        {
            get { return (_uniWinCore != null ? _uniWinCore.GetClientSize() : Vector2.zero); }
        }

        /// <summary>
        /// マウスカーソル座標を取得・設定
        /// </summary>
        public Vector2 cursorPosition
        {
            get { return UniWinCore.GetCursorPosition(); }
            set { UniWinCore.SetCursorPosition(value); }
        }

        /// <summary>
        /// 初期状態でのウィンドウ位置、サイズ
        /// </summary>
        private Rect originalWindowRectangle;

        // カメラの背景をアルファゼロの黒に置き換えるため、本来の背景を保存しておく変数
        private CameraClearFlags originalCameraClearFlags;
        private Color originalCameraBackground;

        /// <summary>
        /// カーソル下1px分の色が入るテクスチャ
        /// </summary>
        private Texture2D colorPickerTexture = null;

        /// <summary>
        /// Raycastで使うマウスイベント情報
        /// </summary>
        private PointerEventData pointerEventData;

        /// <summary>
        /// Raycast 時のレイヤーマスク
        /// </summary>
        private int hitTestLayerMask;
        
        /// <summary>
        /// Occurs when the window style changed
        /// </summary>
        public event OnStateChangedDelegate OnStateChanged;
        public delegate void OnStateChangedDelegate(WindowStateEventType type);

        public delegate void FilesDelegate(string[] files);

        /// <summary>
        /// Occurs after files or folders were dropped
        /// </summary>
        public event FilesDelegate OnDropFiles;

        /// <summary>
        /// Occurs when the monitor settings or resolution changed
        /// </summary>
        public event OnMonitorChangedDelegate OnMonitorChanged;
        public delegate void OnMonitorChangedDelegate();


        // Use this for initialization
        void Awake()
        {
            // シングルトンとする。既にインスタンスがあれば自分を破棄
            if (this != current)
            {
                Destroy(this.gameObject);
                return;
            }
            else
            {
                _current = this;
            }

            // フルスクリーン強制解除。エディタでは何もしない
#if !UNITY_EDITOR
            if (forceWindowed && Screen.fullScreen)
            {
                Screen.fullScreen = false;
            }
#endif

            if (!currentCamera)
            {
                // メインカメラを探す
                currentCamera = Camera.main;

                //// もしメインカメラが見つからなければ、Findで探す
                //if (!currentCamera)
                //{
                //    currentCamera = GameObject.FindObjectOfType<Camera>();
                //}
            }

            // カメラの元の背景を記憶
            if (currentCamera)
            {
                originalCameraClearFlags = currentCamera.clearFlags;
                originalCameraBackground = currentCamera.backgroundColor;

            }
            
            // マウスイベント情報
            pointerEventData = new PointerEventData(EventSystem.current);
            
            // Ignore Raycast 以外を有効とするマスク
            hitTestLayerMask = ~LayerMask.GetMask("Ignore Raycast");

            // マウス下描画色抽出用テクスチャを準備
            colorPickerTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);

            // ウィンドウ制御用のインスタンス作成
            _uniWinCore = new UniWinCore();
        }

        /// <summary>
        /// Fit to specified monitor
        /// </summary>
        private void UpdateMonitorFitting()
        {
            if (!_shouldFitMonitor) return;

            int monitors = UniWinCore.GetMonitorCount();
            int targetMonitorIndex = _monitorToFit;

            if (targetMonitorIndex < 0)
            {
                targetMonitorIndex = 0;
            }
            if (monitors <= targetMonitorIndex)
            {
                targetMonitorIndex = monitors - 1;
            }

            if (targetMonitorIndex >= 0)
            {
                _uniWinCore.FitToMonitor(targetMonitorIndex);
            }
        }

        /// <summary>
        /// Find existing instance or create new instance
        /// </summary>
        /// <returns></returns>
        private static UniWindowController FindOrCreateInstance()
        {
            var instance = GameObject.FindObjectOfType<UniWindowController>();
            
            // 勝手に生成するのは今のところ無効としてみる
            // // シーンに見つからなければ新規作成
            // if (!instance)
            // {
            //     var obj = new GameObject(nameof(UniWindowController));
            //     obj.AddComponent<UniWindowController>();
            // }

            return instance;
        }
        
        void Start()
        {
            // マウスカーソル直下の色を取得するコルーチンを開始
            StartCoroutine(HitTestCoroutine());

            // Get the initial window size and position
            StoreOriginalWindowRectangle();

            // Fit to the selected monitor
            OnMonitorChanged += UpdateMonitorFitting;
            UpdateMonitorFitting();
        }

        void OnDestroy()
        {
            if (_uniWinCore != null)
            {
                _uniWinCore.Dispose();
            }

            // Instance も破棄
            if (this == current)
            {
                _current = null;
            }
        }

        void StoreOriginalWindowRectangle()
        {
            if (_uniWinCore != null)
            {
                var size = _uniWinCore.GetWindowSize();
                var pos = _uniWinCore.GetWindowPosition();
                originalWindowRectangle = new Rect(pos, size);
            }
        }

        // Update is called once per frame
        void Update()
        {
            // 自ウィンドウ取得ができていなければ、取得
            if (_uniWinCore == null || !_uniWinCore.IsActive)
            {
                UpdateTargetWindow();
            } else
            {
                _uniWinCore.Update();
            }
            
            // Process events
            UpdateEvents();

            // キー、マウス操作の下ウィンドウへの透過状態を更新
            UpdateClickThrough();
        }

        /// <summary>
        /// Check and process UniWinCore events
        /// </summary>
        private void UpdateEvents()
        {
            if (_uniWinCore == null) return;

            if (_uniWinCore.ObserveDroppedFiles(out var droppedFiles))
            {
                OnDropFiles?.Invoke(droppedFiles);
            }

            if (_uniWinCore.ObserveMonitorChanged())
            {
                OnMonitorChanged?.Invoke();
            }

            if (_uniWinCore.ObserveWindowStyleChanged(out var type))
            {
                // // モニタへのフィット指定がある状態で最大化解除された場合
                // if (shouldFitMonitor && !uniWinCore.GetZoomed())
                // {
                //     //StartCoroutine("ForceZoomed");    // 時間差で最大化を強制
                //     //SetZoomed(true);        // 強制的に最大化　←必ずしも働かない
                //     //shouldFitMonitor = false;    // フィットを無効化
                // }
                if (_shouldFitMonitor) StartCoroutine("ForceZoomed"); // 時間差で最大化を強制
                
                OnStateChanged?.Invoke((WindowStateEventType)type);
            }
        }

        IEnumerator ForceZoomed()
        {
            yield return new WaitForSeconds(0.5f);
            if (_shouldFitMonitor && !_uniWinCore.GetZoomed()) SetZoomed(true);
            yield return null;
        }

        /// <summary>
        /// カメラを指定。以前のカメラがあれば背景を戻す
        /// </summary>
        /// <param name="newCamera"></param>
        public void SetCamera(Camera newCamera)
        {
            // カメラが変更された場合、設定を戻す
            if (newCamera != currentCamera)
            {
                SetCameraBackground(false);
            }

            currentCamera = newCamera;

            // カメラの元の背景を記憶
            if (currentCamera)
            {
                originalCameraClearFlags = currentCamera.clearFlags;
                originalCameraBackground = currentCamera.backgroundColor;

                SetCameraBackground(_isTransparent);
            }
        }

        /// <summary>
        /// マウス・タッチ操作を下のウィンドウに透過させる
        /// </summary>
        /// <param name="isThrough"></param>
        void SetClickThrough(bool isThrough)
        {
            _uniWinCore?.EnableClickThrough(isThrough);
            _isClickThrough = isThrough;
        }

        /// <summary>
        /// 画素の色を基に操作受付を切り替える
        /// </summary>
        void UpdateClickThrough()
        {
            //　自動ヒットテスト無しならば終了
            if (!isHitTestEnabled || hitTestType == HitTestType.None) return;
            
            // マウスカーソル非表示状態ならば透明画素上と同扱い
            bool hit = (onObject);

            if (_isClickThrough)
            {
                // ここまでクリックスルー状態だったら、ヒットしたときだけ戻す
                if (hit)
                {
                    SetClickThrough(false);
                }
            }
            else
            {
                // ここまでクリックスルーでなければ、透明かつヒットしなかったときだけクリックスルーとする
                if (isTransparent && !hit)
                {
                    SetClickThrough(true);
                }
            }
        }

        /// <summary>
        /// コルーチンでカーソル下の色、またはRaycastによるヒットテストを繰り返す
        /// WaitForEndOfFrame() を使うためにコルーチンとしている
        /// </summary>
        /// <returns></returns>
        private IEnumerator HitTestCoroutine()
        {
            while (Application.isPlaying)
            {
                yield return new WaitForEndOfFrame();

                // Windowsの場合、単色での透過ならばヒットテストはOSに任せるため、常にヒット
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                if (transparentType == TransparentType.ColorKey)
                {
                    onObject = true;
                }
                else
#endif
                if (hitTestType == HitTestType.Opacity)
                {
                    HitTestByOpaquePixel();
                }
                else if (hitTestType == HitTestType.Raycast)
                {
                    HitTestByRaycast();
                }
                else
                {
                    // ヒットテスト無しの場合は常にtrue
                    onObject = true;
                }
            }
            yield return null;
        }

        /// <summary>
        /// マウス下の画素があるかどうかを確認
        /// </summary>
        private void HitTestByOpaquePixel()
        {
            Vector2 mousePos = Input.mousePosition;

            // マウス座標を調べる
            if (GetOnOpaquePixel(mousePos))
            {
                //Debug.Log("Mouse " + mousePos);
                onObject = true;
                //activeFingerId = -1;    // タッチ追跡は解除
                return;
            }
            else
            {
                onObject = false;
            }
        }

        /// <summary>
        /// 指定座標の画素が透明か否かを返す
        /// </summary>
        /// <param name="mousePos">座標[px]。必ず描画範囲内であること。</param>
        /// <returns></returns>
        private bool GetOnOpaquePixel(Vector2 mousePos)
        {
            float w = Screen.width;
            float h = Screen.height;
            //Debug.Log(w + ", " + h);

            // 画面外であれば透明と同様
            if (
                mousePos.x < 0 || mousePos.x >= w
                || mousePos.y < 0 || mousePos.y >= h
                )
            {
                return false;
            }

            // 透過状態でなければ、範囲内なら不透過扱いとする
            if (!_isTransparent) return true;

            // LayeredWindowならばクリックスルーはOSに任せるため、ウィンドウ内ならtrueを返しておく
            if (transparentType == TransparentType.ColorKey) return true;

            // 指定座標の描画結果を見て判断
            try   // WaitForEndOfFrame のタイミングで実行すればtryは無くても大丈夫な気はする
            {
                // Reference http://tsubakit1.hateblo.jp/entry/20131203/1386000440
                colorPickerTexture.ReadPixels(new Rect(mousePos, Vector2.one), 0, 0);
                Color color = colorPickerTexture.GetPixels32()[0];
                pickedColor = color;

                return (color.a >= opacityThreshold);  // αがしきい値以上ならば不透過とする
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// マウス下にオブジェクトがあるかどうかを確認
        /// </summary>
        private void HitTestByRaycast()
        {
            var position = Input.mousePosition;
            
            // // uGUIの上か否かを判定
            var raycastResults = new List<RaycastResult>();
            pointerEventData.position = position;
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);
            foreach (var result in raycastResults)
            {
                // レイヤーマスクを考慮（Ignore Raycast 以外ならヒット）
                if (((1 << result.gameObject.layer) & hitTestLayerMask) > 0)
                {
                    onObject = true;
                    return;
                }
            }
            // レイヤーに関わらずヒットさせる場合は下記でよい
            // // uGUIの上と判定されれば、終了
            // if (EventSystem.current.IsPointerOverGameObject())
            // {
            //     onObject = true;
            //     return;
            // }

            if (currentCamera && currentCamera.isActiveAndEnabled)
            {
                Ray ray = currentCamera.ScreenPointToRay(position);

                // 3Dオブジェクトの上か否かを判定
                if (Physics.Raycast(ray, out _, raycastMaxDepth))
                {
                    onObject = true;
                    return;
                }

                // 2Dオブジェクトの上か判定
                var rayHit2D = Physics2D.GetRayIntersection(ray);
                Debug.DrawRay(ray.origin, ray.direction, Color.blue, 2f, false);
                if (rayHit2D.collider != null)
                {
                    onObject = true;
                    return;
                }
            } else
            {
                // カメラが有効でなければメインカメラを取得
                currentCamera = Camera.main;
            }

            // いずれもヒットしなければオブジェクト上ではないと判断
            onObject = false;
        }

        /// <summary>
        /// 自分のウィンドウハンドルが不確かならば探しなおす
        /// </summary>
        private void UpdateTargetWindow()
        {
            if (_uniWinCore == null)
            {
                _uniWinCore = new UniWinCore();
            }

            // ウィンドウがまだ取得できていなければ、取得の処理を行う
            if (!_uniWinCore.IsActive)
            {
                _uniWinCore.AttachMyWindow();

                // ウィンドウを取得できたら最初の値を設定
                if (_uniWinCore.IsActive)
                {
                    _uniWinCore.SetTransparentType((UniWinCore.TransparentType)transparentType);
                    _uniWinCore.SetKeyColor(keyColor);
                    _uniWinCore.SetAlphaValue(_alphaValue);
                    SetTransparent(_isTransparent);
                    if (_isBottommost)
                    {
                        SetBottommost(_isBottommost);
                    }
                    else
                    {
                        SetTopmost(_isTopmost);
                    }
                    SetZoomed(_isZoomed);
                    SetClickThrough(_isClickThrough);
                    SetAllowDrop(_allowDropFiles);

                    // ウィンドウ取得時にはモニタ変更と同等の処理を行う
                    OnMonitorChanged?.Invoke();
                }
            }
            else
            {
                #if UNITY_EDITOR
                // エディタではゲームビューが閉じられたりドッキングされたりするため、変化していれば対象ウィンドウを変更
                // アクティブウィンドウが現在の対象と同じならばなにもおこらない
                _uniWinCore.AttachMyActiveWindow();
                #endif
            }
        }

        /// <summary>
        /// ウィンドウへのフォーカスが変化したときに呼ばれる
        /// </summary>
        /// <param name="focus"></param>
        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                UpdateTargetWindow();

                // フォーカスが当たった瞬間には、強制的にクリックスルーはオフにする
                if (_isTransparent && isHitTestEnabled && transparentType != TransparentType.ColorKey)
                {
                    SetClickThrough(false);
                }
            }
        }

        /// <summary>
        /// ウィンドウ透過状態になった際、自動的に背景を透明単色に変更する
        /// </summary>
        /// <param name="transparent"></param>
        void SetCameraBackground(bool transparent)
        {
            // カメラが特定できていないか、自動切替をしない場合は、何もしない
            if (!currentCamera || !autoSwitchCameraBackground) return;

            // 透過するならカメラの背景を透明色に変更
            if (transparent)
            {
                currentCamera.clearFlags = CameraClearFlags.SolidColor;
                if (transparentType == TransparentType.ColorKey)
                {
                    currentCamera.backgroundColor = keyColor;
                }
                else
                {
                    currentCamera.backgroundColor = Color.clear;
                }
            }
            else
            {
                currentCamera.clearFlags = originalCameraClearFlags;
                currentCamera.backgroundColor = originalCameraBackground;
            }
        }

        /// <summary>
        /// 透明化状態を切替
        /// </summary>
        /// <param name="transparent"></param>
        private void SetTransparent(bool transparent)
        {
            _isTransparent = transparent;
            SetCameraBackground(transparent);
#if !UNITY_EDITOR
            if (_uniWinCore != null)
            {
                _uniWinCore.EnableTransparent(transparent);
            }
#endif
            UpdateClickThrough();
        }

        /// <summary>
        /// 透過方法を変更
        /// </summary>
        /// <param name="type"></param>
        public void SetTransparentType(TransparentType type)
        {
            if (_uniWinCore != null) {
                // 透過中だったなら、一度解除して再透過
                if (_isTransparent)
                {
                    SetTransparent(false);
                    _uniWinCore.SetTransparentType((UniWinCore.TransparentType)type);
                    transparentType = type;
                    SetTransparent(true);
                }
                else
                {
                    _uniWinCore.SetTransparentType((UniWinCore.TransparentType)type);
                    transparentType = type;
                }
            }
        }

        /// <summary>
        /// Set window alpha
        /// </summary>
        /// <param name="alpha">0.0 to 1.0</param>
        private void SetAlphaValue(float alpha)
        {
            _alphaValue = alpha;
            _uniWinCore?.SetAlphaValue(_alphaValue);
        }

        /// <summary>
        /// 最前面を切替
        /// </summary>
        /// <param name="topmost"></param>
        private void SetTopmost(bool topmost)
        {
            //if (_isTopmost == topmost) return;
            if (_uniWinCore == null) return;

            _uniWinCore.EnableTopmost(topmost);
            _isTopmost = _uniWinCore.IsTopmost;
            _isBottommost = _uniWinCore.IsBottommost;
        }

        /// <summary>
        /// 常に最背面を切替
        /// </summary>
        /// <param name="bottommost"></param>
        private void SetBottommost(bool bottommost)
        {
            if (_uniWinCore == null) return;

            _uniWinCore.EnableBottommost(bottommost);
            _isBottommost = _uniWinCore.IsBottommost;
            _isTopmost = _uniWinCore.IsTopmost;
        }

        /// <summary>
        /// 最大化する
        /// </summary>
        /// <param name="zoomed"></param>
        private void SetZoomed(bool zoomed)
        {
            if (_uniWinCore == null) return;

            _uniWinCore.SetZoomed(zoomed);
            _isZoomed = _uniWinCore.GetZoomed();
        }

        private void SetAllowDrop(bool enabled)
        {
            if (_uniWinCore == null) return;

            _uniWinCore.SetAllowDrop(enabled);
            _allowDropFiles = enabled;
        }


        /// <summary>
        /// Get the number of connected monitors
        /// </summary>
        /// <returns></returns>
        public static int GetMonitorCount()
        {
            //if (uniWinCore == null) return 0;
            return UniWinCore.GetMonitorCount();
        }

        /// <summary>
        /// Get monitor position and size
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Rect GetMonitorRect(int index)
        {
            if (UniWinCore.GetMonitorRectangle(index, out Vector2 position, out Vector2 size))
            {
                return new Rect(position, size);
            }
            return Rect.zero;
        }

        /// <summary>
        /// Fit to the specified monitor
        /// </summary>
        /// <returns></returns>
        private bool FitToMonitor(bool shouldFit, int monitorIndex)
        {
            if (_uniWinCore == null)
            {
                _shouldFitMonitor = shouldFit;
                _monitorToFit = monitorIndex;
                return false;
            }

            if (shouldFit)
            {
                if (!_shouldFitMonitor)
                {
                    // 直前はフィットしない状態だった場合
                    _monitorToFit = monitorIndex;
                    _shouldFitMonitor = shouldFit;
                    UpdateMonitorFitting();
                }
                else
                {
                    if (_monitorToFit != monitorIndex)
                    {
                        // フィット先モニタが変化した場合
                        _monitorToFit = monitorIndex;
                        UpdateMonitorFitting();
                    }
                }
            } 
            else
            {
                if (_shouldFitMonitor)
                {
                    // 直前はフィット状態で、解除された場合
                    _monitorToFit = monitorIndex;
                    _shouldFitMonitor = shouldFit;
                    UpdateMonitorFitting();

                    _uniWinCore.SetZoomed(false);
                    //uniWinCore.SetWindowSize(originalWindowRectangle.size);
                    //uniWinCore.SetWindowPosition(originalWindowRectangle.position);
                }
                else
                {
                    // フィット中でなければ選択を変えるのみ
                    _monitorToFit = monitorIndex;
                }
            }

            return true;
        }

        /// <summary>
        /// Get mouse cursor position
        /// </summary>
        /// <returns>Cursor position</returns>
        public static Vector2 GetCursorPosition()
        {
            return UniWinCore.GetCursorPosition();
        }
        
        /// <summary>
        /// Set mouse cursor position
        /// </summary>
        /// <param name="position"></param>
        public static void SetCursorPosition(Vector2 position)
        {
            UniWinCore.SetCursorPosition(position);
        }

        /// <summary>
        /// 終了時にはウィンドウ状態を戻す処理が必要
        /// </summary>
        void OnApplicationQuit()
        {
            if (Application.isPlaying)
            {
                if (_uniWinCore != null)
                {
                    // エディタだとウィンドウ状態を戻す
                    // スタンドアローンだと戻した姿が見えてしまうためスキップ
#if UNITY_EDITOR
                    _uniWinCore.SetWindowSize(originalWindowRectangle.size);
                    _uniWinCore.SetWindowPosition(originalWindowRectangle.position);

                    _uniWinCore.DetachWindow();
#endif
                    _uniWinCore.Dispose();
                }
            }
        }

        /// <summary>
        /// 自分のウィンドウにフォーカスを与える
        /// </summary>
        public void Focus()
        {
            if (_uniWinCore != null)
            {
                //uniWin.SetFocus();
            }
        }


        /// <summary>
        /// デバッグ専用。その都度参考となる情報を受けるための関数
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public int GetDebugInfo()
        {
            if (_uniWinCore != null) {
                return UniWinCore.GetDebugInfo();
            }
            return 0;
        }
    }
}
