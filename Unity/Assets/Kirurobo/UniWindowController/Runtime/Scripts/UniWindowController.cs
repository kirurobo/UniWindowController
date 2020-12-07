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
    /// <summary>
    /// Set editable the bool property
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class BoolPropertyAttribute : PropertyAttribute { }

    /// <summary>
    /// Set the attribute as readonly
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ReadOnlyAttribute : PropertyAttribute { }

    /// <summary>
    /// ウィンドウ操作をとりまとめるクラス
    /// </summary>
    public class UniWindowController : MonoBehaviour
    {
        /// <summary>
        /// 透明化の方式
        /// </summary>
        public enum HitTestType
        {
            None = 0,
            Opacity = 1,
            Raycast = 2,
        }
        
        /// <summary>
        /// Low level class
        /// </summary>
        private UniWinCore uniWinCore = null;

        /// <summary>
        /// Is this window receives mouse events
        /// </summary>
        public bool isClickThrough
        {
            get { return _isClickThrough; }
            set { SetClickThrough(value); }
        }
        private bool _isClickThrough = true;

        /// <summary>
        /// Is this window transparent
        /// </summary>
        public bool isTransparent
        {
            get { return _isTransparent; }
            set { SetTransparent(value); }
        }
        [SerializeField, BoolProperty, Tooltip("Check to set transparent on startup")]
        private bool _isTransparent = false;

        /// <summary>
        /// Is this window minimized
        /// </summary>
        public bool isTopmost
        {
            get { return ((uniWinCore == null) ? _isTopmost : _isTopmost = uniWinCore.IsTopmost); }
            set { SetTopmost(value); }
        }
        [SerializeField, BoolProperty, Tooltip("Check to set topmost on startup")]
        private bool _isTopmost = false;

        /// <summary>
        /// Is this window minimized
        /// </summary>
        public bool isZoomed
        {
            get { return ((uniWinCore == null) ? _isZoomed : _isZoomed = uniWinCore.GetZoomed()); }
            set { SetZoomed(value); }
        }
        [SerializeField, BoolProperty, Tooltip("Experimental...")]
        private bool _isZoomed = false;

        /// <summary>
        /// Always fit to monitor (0, 1, ...)
        /// Disabled if the value is -1
        /// </summary>
        public int fitToMonitor = -1;

        /// <summary>
        /// Enable / disable accepting file drop
        /// </summary>
        public bool allowDropFiles
        {
            get { return _allowDropFiles; }
            set { SetAllowDrop(value); }
        }
        [SerializeField, BoolProperty, Tooltip("Experimental...")]
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
        /// 透過方式の指定
        /// </summary>
        [Header("For Windows only")]
        [Tooltip("Select the method. *Only available on Windows")]
        public UniWinCore.TransparentType transparentType = UniWinCore.TransparentType.Alpha;

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
            get { return (uniWinCore != null ? uniWinCore.GetWindowPosition() : Vector2.zero); }
            set { uniWinCore?.SetWindowPosition(value); }
        }

        /// <summary>
        /// ウィンドウ座標を取得・設定
        /// </summary>
        public Vector2 windowSize
        {
            get { return (uniWinCore != null ? uniWinCore.GetWindowSize() : Vector2.zero); }
            set { uniWinCore?.SetWindowSize(value); }
        }

        /// <summary>
        /// マウスカーソル座標を取得・設定
        /// </summary>
        public Vector2 cursorPosition
        {
            get { return (uniWinCore != null ? uniWinCore.GetCursorPosition() : Vector2.zero); }
            set { uniWinCore?.SetCursorPosition(value); }
        }

        // カメラの背景をアルファゼロの黒に置き換えるため、本来の背景を保存しておく変数
        private CameraClearFlags originalCameraClearFlags;
        private Color originalCameraBackground;

        /// <summary>
        /// カメラのインスタンス
        /// </summary>
        private Camera currentCamera;

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
        /// ウィンドウ状態が変化したときに発生するイベント
        /// </summary>
        public event OnStateChangedDelegate OnStateChanged;

        public delegate void OnStateChangedDelegate();

        public event OnDropFilesDelegate OnDropFiles;

        public event OnDisplayChangedDelegate OnDisplayChanged;

        public delegate void OnDropFilesDelegate(string[] files);

        public delegate void OnDisplayChangedDelegate();


        // Use this for initialization
        void Awake()
        {
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

                // もしメインカメラが見つからなければ、Findで探す
                if (!currentCamera)
                {
                    currentCamera = GameObject.FindObjectOfType<Camera>();
                }
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
            uniWinCore = new UniWinCore();
        }

        /// <summary>
        /// Fit to specified monitor
        /// </summary>
        private void UpdateMonitorFitting()
        {
            if (fitToMonitor < 0) return;

            int monitors = uniWinCore.GetMonitorCount();
            int targetMonitorIndex = fitToMonitor;

            if (monitors <= targetMonitorIndex)
            {
                targetMonitorIndex = monitors - 1;
            }

            if (targetMonitorIndex >= 0)
            {
                uniWinCore.FitToMonitor(targetMonitorIndex);
            }
        }
        
        void Start()
        {
            // マウスカーソル直下の色を取得するコルーチンを開始
            StartCoroutine(HitTestCoroutine());

            // Fit to the selected monitor
            OnDisplayChanged += UpdateMonitorFitting;
            UpdateMonitorFitting();
        }

        void OnDestroy()
        {
            if (uniWinCore != null)
            {
                uniWinCore.Dispose();
            }
        }

        // Update is called once per frame
        void Update()
        {
            // 自ウィンドウ取得ができていなければ、取得
            if (uniWinCore == null || !uniWinCore.IsActive)
            {
                UpdateTargetWindow();
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
            if (uniWinCore == null) return;

            if (uniWinCore.FetchDroppedFiles(out var files))
            {
                OnDropFiles?.Invoke(files);
            }

            if (uniWinCore.FetchDisplayChanged())
            {
                OnDisplayChanged?.Invoke();
            }
        }

        /// <summary>
        /// ウィンドウ状態が変わったときに呼ぶイベントを処理
        /// </summary>
        private void StateChangedEvent()
        {
            OnStateChanged?.Invoke();
        }

        /// <summary>
        /// マウス・タッチ操作を下のウィンドウに透過させる
        /// </summary>
        /// <param name="isThrough"></param>
        void SetClickThrough(bool isThrough)
        {
            uniWinCore?.EnableClickThrough(isThrough);
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
                if (transparentType == UniWinCore.TransparentType.ColorKey)
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
        /// <param name="cam"></param>
        private void HitTestByOpaquePixel()
        {
            Vector2 mousePos;
            mousePos = Input.mousePosition;

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
            if (transparentType == UniWinCore.TransparentType.ColorKey) return true;

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

            // 3Dオブジェクトの上か否かを判定
            Ray ray = currentCamera.ScreenPointToRay(position);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, raycastMaxDepth))
            {
                onObject = true;
            }
            else
            {
                onObject = false;
            }
        }

        /// <summary>
        /// 自分のウィンドウハンドルが不確かならば探しなおす
        /// </summary>
        private void UpdateTargetWindow()
        {
            if (uniWinCore == null)
            {
                uniWinCore = new UniWinCore();
            }

            // ウィンドウがまだ取得できていなければ、取得の処理を行う
            if (!uniWinCore.IsActive)
            {
                uniWinCore.AttachMyWindow();

                // ウィンドウを取得できたら最初の値を設定
                if (uniWinCore.IsActive)
                {
                    uniWinCore.SetTransparentType(transparentType);
                    uniWinCore.SetKeyColor(keyColor);
                    SetTransparent(_isTransparent);
                    SetTopmost(_isTopmost);
                    SetClickThrough(_isClickThrough);
                    SetAllowDrop(_allowDropFiles);
                }
            }
            else
            {
                #if UNITY_EDITOR
                // エディタではゲームビューが閉じられたりドッキングされたりするため、変化していれば対象ウィンドウを変更
                // アクティブウィンドウが現在の対象と同じならばなにもおこらない
                uniWinCore.AttachMyActiveWindow();
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
                if (_isTransparent && isHitTestEnabled && transparentType != UniWinCore.TransparentType.ColorKey)
                {
                    SetClickThrough(false);
                }
            }
        }

        /// <summary>
        /// ウィンドウ透過状態になった際、自動的に背景を透明単色に変更する
        /// </summary>
        /// <param name="isTransparent"></param>
        void SetCameraBackground(bool isTransparent)
        {
            // カメラが特定できていないか、自動切替をしない場合は、何もしない
            if (!currentCamera || !autoSwitchCameraBackground) return;

            // 透過するならカメラの背景を透明色に変更
            if (isTransparent)
            {
                currentCamera.clearFlags = CameraClearFlags.SolidColor;
                if (transparentType == UniWinCore.TransparentType.ColorKey)
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
            //if (_isTransparent == transparent) return;

            _isTransparent = transparent;
            SetCameraBackground(transparent);
#if !UNITY_EDITOR
            if (uniWinCore != null)
            {
                uniWinCore.EnableTransparent(transparent);
            }
#endif
            UpdateClickThrough();
            StateChangedEvent();
        }

        /// <summary>
        /// 透過方法を変更
        /// </summary>
        /// <param name="type"></param>
        public void SetTransparentType(UniWinCore.TransparentType type)
        {
            if (uniWinCore != null) {
                // 透過中だったなら、一度解除して再透過
                if (_isTransparent)
                {
                    SetTransparent(false);
                    uniWinCore.SetTransparentType(type);
                    transparentType = type;
                    SetTransparent(true);
                }
                else
                {
                    uniWinCore.SetTransparentType(type);
                    transparentType = type;
                }
            }
        }

        /// <summary>
        /// 最前面を切替
        /// </summary>
        /// <param name="topmost"></param>
        private void SetTopmost(bool topmost)
        {
            //if (_isTopmost == topmost) return;
            if (uniWinCore == null) return;

            uniWinCore.EnableTopmost(topmost);
            _isTopmost = uniWinCore.IsTopmost;
            StateChangedEvent();
        }

        /// <summary>
        /// 最大化する
        /// </summary>
        /// <param name="zoomed"></param>
        private void SetZoomed(bool zoomed)
        {
            if (uniWinCore == null) return;

            uniWinCore.SetZoomed(zoomed);
            _isZoomed = uniWinCore.GetZoomed();
            StateChangedEvent();
        }

        private void SetAllowDrop(bool enabled)
        {
            if (uniWinCore == null) return;

            uniWinCore.SetAllowDrop(enabled);
            _allowDropFiles = enabled;
            StateChangedEvent();
        }

        /// <summary>
        /// 接続されているモニタ数を取得
        /// </summary>
        /// <returns></returns>
        public int GetMonitorCount()
        {
            if (uniWinCore == null) return 0;
            return uniWinCore.GetMonitorCount();
        }

        /// <summary>
        /// 接続されているモニタ数を取得
        /// </summary>
        /// <returns></returns>
        public bool FitToMonitor(int monitorIndex)
        {
            if (uniWinCore == null) return false;
            return uniWinCore.FitToMonitor(monitorIndex);
        }

        /// <summary>
        /// 終了時にはウィンドウ状態を戻す処理が必要
        /// </summary>
        void OnApplicationQuit()
        {
            if (Application.isPlaying)
            {
                if (uniWinCore != null)
                {
                    // エディタだとウィンドウ状態を戻す
                    // スタンドアローンだと戻した姿が見えてしまうためスキップ
#if UNITY_EDITOR
                    uniWinCore.DetachWindow();
#endif
                    uniWinCore.Dispose();
                }
            }
        }

        /// <summary>
        /// 自分のウィンドウにフォーカスを与える
        /// </summary>
        public void Focus()
        {
            if (uniWinCore != null)
            {
                //uniWin.SetFocus();
            }
        }
    }
}
