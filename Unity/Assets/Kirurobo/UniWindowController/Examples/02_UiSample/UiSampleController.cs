/**
 * A sample script of UniWindowContoller
 * 
 * Author: Kirurobo http://twitter.com/kirurobo
 * License: MIT
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Kirurobo
{
    /// <summary>
    /// WindowControllerの設定をToggleでオン／オフするサンプル
    /// </summary>
    public class UiSampleController : MonoBehaviour
    {
        private UniWindowController uniwinc;
        private UniWindowMoveHandle uniWinMoveHandle;

        public Toggle transparentToggle;
        public Toggle topmostToggle;
        public Toggle dragMoveToggle;
        public Toggle maximizedToggle;
        public Dropdown transparentTypeDropdown;
        public Dropdown hitTestTypeDropdown;
        public Toggle clickThroughToggle;
        public Image pickedColorImage;
        public Text pickedColorText;
        public Text messageText;
        public Button menuCloseButton;
        public RectTransform menuPanel;

        /// <summary>
        /// 初期化
        /// </summary>
        void Start()
        {
            // UniWindowController を探す
            uniwinc = GameObject.FindObjectOfType<UniWindowController>();
            
            // UniWindowDragMove を探す
            uniWinMoveHandle = GameObject.FindObjectOfType<UniWindowMoveHandle>();
            
            // Toggleのチェック状態を、現在の状態に合わせる
            UpdateUI();

            if (uniwinc)
            {
                // UIを操作された際にはウィンドウに反映されるようにする
                transparentToggle?.onValueChanged.AddListener(val => uniwinc.isTransparent = val);
                topmostToggle?.onValueChanged.AddListener(val => uniwinc.isTopmost = val);
                maximizedToggle?.onValueChanged.AddListener(val => uniwinc.isZoomed = val);
                clickThroughToggle?.onValueChanged.AddListener(val => uniwinc.isClickThrough = val);

                transparentTypeDropdown?.onValueChanged.AddListener(val => uniwinc.SetTransparentType((UniWinCore.TransparentType)val));
                hitTestTypeDropdown?.onValueChanged.AddListener(val => uniwinc.hitTestType = (UniWindowController.HitTestType)val);
                menuCloseButton?.onClick.AddListener(CloseMenu);

                if (uniWinMoveHandle) dragMoveToggle?.onValueChanged.AddListener(val => uniWinMoveHandle.enabled = val);

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                // Windows でなければ、透過方法の選択は無効とする
                //if (transparentTypeDropdown) transparentTypeDropdown.interactable = false;
                if (transparentTypeDropdown) transparentTypeDropdown.enabled = false;
#endif
            }
        }

        /// <summary>
        /// 毎フレーム行う処理
        /// </summary>
        private void Update()
        {
            // ヒットテスト関連の表示を更新
            UpdateHitTestUI();
            
            // 動作確認のためウィンドウ位置・サイズを表示
            ShowWindowMetrics();

            // 右クリックでメニューを表示
            if (Input.GetMouseButtonDown(1))
            {
                ShowMenu();
            }

            if (uniwinc)
            {
                // [Space]キーを押すと強制的にクリックスルーを解除
                // 操作不能となったときの対応
                // ただし自動判定が有効ならすぐ変化の可能性もある
                if (Input.GetKeyUp(KeyCode.Space))
                {
                    uniwinc.isClickThrough = false;
                }

                // ウィンドウサイズ変更のテスト。カーソルやジョイスティックで伸び縮み
                Vector2 size = uniwinc.windowSize;
                float deltaW = 0f;  // 横に伸ばす幅 [px]
                float deltaH = 0f;  // 縦に伸ばす高さ [px]
                const float step = 10f; // 1フレームでの変化量 [px]
                deltaW = Input.GetAxis("Horizontal") * step;
                deltaH = Input.GetAxis("Vertical") * step;

                if (!Mathf.Approximately(deltaW, 0f) || !Mathf.Approximately(deltaH, 0f))
                {
                    uniwinc.windowSize += new Vector2(deltaW, deltaH);
                }
            }

            // Quit or stop playing when pressed [ESC]
            if (Input.GetKey(KeyCode.Escape))
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }

        /// <summary>
        /// ウィンドウ位置と座標を表示
        /// </summary>
        void ShowWindowMetrics()
        {
            if (uniwinc)
            {
                var winPos = uniwinc.windowPosition;
                OutputMessage(
                    "Pos.: " + winPos
                    + "\nSize: " + uniwinc.windowSize
                    + "\nRel. Cur.:" + (uniwinc.cursorPosition - winPos)
                    + "\nUnity Cur.:" + (Vector2)Input.mousePosition
                    );
            }
        }

        /// <summary>
        /// Refresh UI on focused
        /// </summary>
        /// <param name="hasFocus"></param>
        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                UpdateUI();

                if (uniwinc)
                {
                    OutputMessage("Focused");
                }
                else
                {
                    OutputMessage("No UniWindowController");
                }
                
            }
        }

        private void ShowMenu()
        {
            if (menuPanel)
            {
                menuPanel.gameObject.SetActive(true);
            }
        }
        private void CloseMenu()
        {
            if (menuPanel)
            {
                menuPanel.gameObject.SetActive(false);
            }
        } 

        /// <summary>
        /// 実際の状態をUI表示に反映
        /// </summary>
        private void UpdateUI()
        {
            if (uniwinc)
            {
                if (transparentToggle)
                {
                    transparentToggle.isOn = uniwinc.isTransparent;
                }

                if (topmostToggle)
                {
                    topmostToggle.isOn = uniwinc.isTopmost;
                }

                if (maximizedToggle)
                {
                    maximizedToggle.isOn = uniwinc.isZoomed;
                }

                if (dragMoveToggle)
                {
                    dragMoveToggle.isOn = (uniWinMoveHandle && uniWinMoveHandle.isActiveAndEnabled);
                }

                if (transparentTypeDropdown)
                {
                    transparentTypeDropdown.value = (int)uniwinc.transparentType;
                    transparentTypeDropdown.RefreshShownValue();
                }


                if (hitTestTypeDropdown)
                {
                    hitTestTypeDropdown.value = (int)uniwinc.hitTestType;
                    hitTestTypeDropdown.RefreshShownValue();
                }
                
                // ヒットテスト部分の表示も更新
                UpdateHitTestUI();
            }
        }

        /// <summary>
        /// ヒットテスト関連のUI更新
        /// 自動で変化するため UpdateUI() よりも高頻度で更新の必要がある
        /// </summary>
        public void UpdateHitTestUI()
        {
            if (uniwinc)
            {
                if (clickThroughToggle)
                {
                    clickThroughToggle.isOn = uniwinc.isClickThrough;
                    if (uniwinc.hitTestType == UniWindowController.HitTestType.None)
                    {
                        clickThroughToggle.interactable = true;
                    }
                    else
                    {
                        clickThroughToggle.interactable = false;
                    }
                }

                if (uniwinc.hitTestType == UniWindowController.HitTestType.Opacity && uniwinc.isTransparent)
                {
                    if (pickedColorImage)
                    {
                        pickedColorImage.color = uniwinc.pickedColor;
                    }

                    if (pickedColorText)
                    {
                        pickedColorText.text = $"Alpha:{uniwinc.pickedColor.a:P0}";
                        pickedColorText.color = Color.black;
                    }
                }
                else
                {
                    if (pickedColorImage)
                    {
                        pickedColorImage.color = Color.gray;
                    }

                    if (pickedColorText)
                    {
                        pickedColorText.text = $"Color picker is disabled";
                        pickedColorText.color = Color.gray;
                    }
                }
            }
        }

        /// <summary>
        /// テキスト枠がUIにあれば、そこにメッセージを出す。無ければコンソールに出力
        /// </summary>
        /// <param name="text"></param>
        public void OutputMessage(string text)
        {
            if (messageText)
            {
                messageText.text = text;
            }
            else
            {
                Debug.Log(text);
            }
        }
    }
}
