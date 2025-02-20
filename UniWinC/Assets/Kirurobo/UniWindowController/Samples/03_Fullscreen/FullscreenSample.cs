/**
* UI controller for the fullscreen sample
* 
* Author: Kirurobo http://twitter.com/kirurobo
* License: MIT
*/

using UnityEngine;
using UnityEngine.UI;

namespace Kirurobo
{
    /// <summary>
    /// WindowControllerの設定をToggleでオン／オフするサンプル
    /// </summary>
    public class FullscreenSample : MonoBehaviour
    {
        private UniWindowController uniwinc;
        private RectTransform canvasRect;

        private float mouseMoveSS = 0f;           // Sum of mouse trajectory squares. [px^2]
        private float mouseMoveSSThreshold = 36f; // Click (not dragging) threshold. [px^2]
        private Vector3 lastMousePosition;        // Right clicked position.
        private float touchDuration = 0f;
        private float touchDurationThreshold = 0.5f;   // Long tap time threshold. [s]

        public Toggle transparentToggle;
        public Toggle topmostToggle;
        public Toggle bottommostToggle;
        public Dropdown fitWindowDropdown;
        public Button quitButton;
        public Button menuCloseButton;
        public RectTransform menuPanel;

        /// <summary>
        /// Setup
        /// </summary>
        void Start()
        {
            // UniWindowController を探す
            uniwinc = GameObject.FindAnyObjectByType<UniWindowController>();

            // CanvasのRectTransform取得
            if (menuPanel) canvasRect = menuPanel.GetComponentInParent<Canvas>().GetComponent<RectTransform>();

            // 有効なモニタ数に合わせて選択肢を作成
            UpdateMonitorDropdown();

            // Toggleのチェック状態を、現在の状態に合わせる
            UpdateUI();
            
            // 初期状態ではメニューを閉じておく
            CloseMenu();

            if (uniwinc)
            {
                // UIを操作された際にはウィンドウに反映されるようにする
                transparentToggle?.onValueChanged.AddListener(val => uniwinc.isTransparent = val);
                topmostToggle?.onValueChanged.AddListener(val => uniwinc.isTopmost = val);
                bottommostToggle?.onValueChanged.AddListener(val => uniwinc.isBottommost = val);
                fitWindowDropdown?.onValueChanged.AddListener(val => SetFitToMonitor(val));
                quitButton?.onClick.AddListener(Quit);
                menuCloseButton?.onClick.AddListener(CloseMenu);

                // Add events
                uniwinc.OnStateChanged += (type) =>
                {
                    UpdateUI();
                    //ShowEventMessage("Window state changed: " + type);
                };
                uniwinc.OnMonitorChanged += () => {
                    UpdateMonitorDropdown();
                    UpdateUI();
                    //ShowEventMessage("Resolution changed!");
                };
            }
        }

        /// <summary>
        /// Perform every frame
        /// </summary>
        private void Update()
        {
            // Show the context menu when right clicked.
            // If mouse movement is closer than a threshold, it is considered a click
            if (InputProxy.GetMouseButtonDown(1))
            {
                lastMousePosition = InputProxy.mousePosition;
                touchDuration = 0f;
            }
            if (InputProxy.GetMouseButton(1))
            {
                mouseMoveSS += (InputProxy.mousePosition - lastMousePosition).sqrMagnitude;
            }
            if (InputProxy.GetMouseButtonUp(1))
            {
                if (mouseMoveSS < mouseMoveSSThreshold)
                {
                    ShowMenu(lastMousePosition);
                }
                mouseMoveSS = 0f;
                touchDuration = 0f;
            }

            // ひとまず Legacy Input Manager でのみタッチ処理を扱う
            #if ENABLE_LEGACY_INPUT_MANAGER
            // Show the menu also when long touched
            if (Input.touchSupported && (Input.touchCount > 0))
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    lastMousePosition = Input.mousePosition;
                    touchDuration = 0f;
                }
                if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                {
                    mouseMoveSS += touch.deltaPosition.sqrMagnitude;
                    touchDuration += touch.deltaTime;
                }
                if (touch.phase == TouchPhase.Ended)
                {
                    if ((mouseMoveSS < mouseMoveSSThreshold) && (touchDuration >= touchDurationThreshold))
                    {
                        ShowMenu(lastMousePosition);
                    }
                    mouseMoveSS = 0f;
                    touchDuration = 0f;
                }
            }
            #endif

            // Show the menu also when pressed [Space] key
            if (InputProxy.GetKeyUp("space"))
            {
                if (menuPanel)
                {
                    if (menuPanel.gameObject.activeSelf) {
                        CloseMenu();
                    } else {
                        Vector2 pos = new Vector2(Screen.width / 2, Screen.height / 2);
                        ShowMenu(pos);
                    }
                }
            }

            // Quit or stop playing when pressed [ESC]
            if (InputProxy.GetKeyUp("escape"))
            {
                Quit();
            }
        }

        void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// フィット対象モニタドロップダウンが変更された時の処理
        /// </summary>
        /// <param name="val"></param>
        void SetFitToMonitor(int val)
        {
            if (!uniwinc) return;

            if (val < 1)
            {
                // ドロップダウンの先頭は、フィット無し
                uniwinc.shouldFitMonitor = false;
            }
            else
            {
                // 次からなので、モニタ番号は1を引く
                uniwinc.monitorToFit = val - 1;
                uniwinc.shouldFitMonitor = true;    // これがfalseからtrueにしたタイミングで移動されるため、モニタ番号を指定してから変更
            }
        }

        /// <summary>
        /// 指定した座標にコンテキストメニューを表示する
        /// </summary>
        /// <param name="position">中心座標指定</param>
        private void ShowMenu(Vector2 position)
        {
            if (menuPanel)
            {
                Vector2 pos = position * (canvasRect.sizeDelta.x / Screen.width);
                float w = menuPanel.rect.width;
                float h = menuPanel.rect.height;

                // 指定座標に中心が来る前提で位置調整
                pos.y = Mathf.Max(Mathf.Min(pos.y, Screen.height - h / 2f), h / 2f);   // はみ出していれば上に寄せる
                pos.x = Mathf.Max(Mathf.Min(pos.x, Screen.width - w / 2f), w / 2f);    // 右にはみ出していれば左に寄せる

                menuPanel.pivot = Vector2.one * 0.5f;    // Set the center
                menuPanel.anchorMin = Vector2.zero;
                menuPanel.anchorMax = Vector2.zero;
                menuPanel.anchoredPosition = pos;
                
                menuPanel.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// コンテキストメニューを閉じる
        /// </summary>
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

                if (bottommostToggle)
                {
                    bottommostToggle.isOn = uniwinc.isBottommost;
                }

                if (fitWindowDropdown)
                {
                    if (uniwinc.shouldFitMonitor)
                    {
                        fitWindowDropdown.value = uniwinc.monitorToFit + 1;
                    }
                    else
                    {
                        fitWindowDropdown.value = 0;
                    }
                    fitWindowDropdown.RefreshShownValue();
                }
            }
        }

        /// <summary>
        /// モニタ選択ドロップダウンの選択肢を更新
        /// この後にUpdateUI()を呼ぶこと
        /// </summary>
        void UpdateMonitorDropdown()
        {
            if (!fitWindowDropdown) return;

            // 先頭以外の選択肢を削除
            fitWindowDropdown.options.RemoveRange(1, fitWindowDropdown.options.Count - 1);

            if (!uniwinc)
            {
                fitWindowDropdown.value = 0;
            }
            else
            {
                int count = UniWindowController.GetMonitorCount();
                for (int i = 0; i < count; i++)
                {
                    fitWindowDropdown.options.Add(new Dropdown.OptionData("Fit to Monitor " + i));
                }
                if (uniwinc.monitorToFit >= count)
                {
                    uniwinc.monitorToFit = count - 1;
                }
            }
        }

        /// <summary>
        /// Show the message with timeout
        /// </summary>
        /// <param name="message"></param>
        private void ShowEventMessage(string message)
        {
            Debug.Log(message);
        }

        /// <summary>
        /// テキスト枠がUIにあれば、そこにメッセージを出す。無ければコンソールに出力
        /// </summary>
        /// <param name="text"></param>
        public void OutputMessage(string text)
        {
              Debug.Log(text);
        }
    }
}
