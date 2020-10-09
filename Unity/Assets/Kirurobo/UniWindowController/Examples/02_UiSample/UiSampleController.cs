using System;
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
        private UniWindowDragMove uniwinDragMove;

        public Toggle transparentToggle;
        public Toggle topmostToggle;
        public Toggle dragMoveToggle;
        public Dropdown hitTestTypeDropdown;
        public Toggle clickThroughToggle;
        public Image pickedColorImage;
        public Text pickedColorText;
        public Text messageText;
        public Button menuCloseButton;
        public RectTransform menuPanel;

        // Use this for initialization
        void Start()
        {
            // UniWindowController を探す
            uniwinc = GameObject.FindObjectOfType<UniWindowController>();
            
            // UniWindowDragMove を探す
            uniwinDragMove = GameObject.FindObjectOfType<UniWindowDragMove>();
            
            // Toggleのチェック状態を、現在の状態に合わせる
            UpdateUI();

            // Toggleを操作された際にはウィンドウに反映されるようにする
            if (transparentToggle)
            {
                transparentToggle.onValueChanged.AddListener(val => uniwinc.isTransparent = val);
            }
            if (topmostToggle)
            {
                topmostToggle.onValueChanged.AddListener(val => uniwinc.isTopmost = val);
            }
            if (dragMoveToggle && uniwinDragMove)
            {
                dragMoveToggle.onValueChanged.AddListener(val => uniwinDragMove.enabled = val);
            }
            if (clickThroughToggle)
            {
                clickThroughToggle.onValueChanged.AddListener(val => uniwinc.isClickThrough = val);
            }

            if (hitTestTypeDropdown)
            {
                hitTestTypeDropdown.onValueChanged.AddListener(val => uniwinc.hitTestType = (UniWindowController.HitTestType)val);
            }
            if (menuCloseButton)
            {
                menuCloseButton.onClick.AddListener(CloseMenu);
            }
        }

        /// <summary>
        /// 
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
            
            // [Space]キーを押すと強制的にクリックスルーを解除
            // 操作不能となったときの対応
            // ただし自動判定が有効ならすぐ変化の可能性もある
            if (Input.GetKeyUp(KeyCode.Space))
            {
                if (uniwinc) uniwinc.isClickThrough = false;
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
                OutputMessage(
                    "Window"
                    + "\nPos.: " + uniwinc.windowPosition
                    + "\nSize: " + uniwinc.windowSize
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
                    var pos = uniwinc.windowPosition;
                    var size = uniwinc.windowSize;
                    OutputMessage("Window\nPos.:" + pos + "Size:" + size);
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

                if (dragMoveToggle)
                {
                    dragMoveToggle.isOn = (uniwinDragMove && uniwinDragMove.isActiveAndEnabled);
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
