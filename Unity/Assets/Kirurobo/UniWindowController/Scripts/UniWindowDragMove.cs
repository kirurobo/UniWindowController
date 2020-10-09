using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  Kirurobo
{
    public class UniWindowDragMove : MonoBehaviour
    {
        private UniWindowController uniwinc;

        /// <summary>
        /// ドラッグ中なら true
        /// </summary>
        public bool isDragging
        {
            get { return _isDragging; }
        }
        private bool _isDragging = false;

        /// <summary>
        /// ドラッグ前には自動ヒットテストが有効だったかを記憶
        /// </summary>
        private bool isHitTestEnabled;
        
        /// <summary>
        /// ドラッグ開始時のウィンドウ内座標[px]
        /// </summary>
        private Vector2 dragStartedPosition;

        /// <summary>
        /// 描画の上にタッチがあればそのfingerIdが入る
        /// </summary>
        //[SerializeField, ReadOnly]
        private int activeFingerId = -1;

        /// <summary>
        /// タッチがBeganとなったものを受け渡すためのリスト
        /// PickColorCoroutine()実行のタイミングではどうもtouch.phaseがうまくとれないようなのでこれで渡してみる
        /// </summary>
        private Touch? firstTouch = null;

        /// <summary>
        /// 最後のドラッグはマウスによるものか、タッチによるものか
        /// </summary>
        //[SerializeField, ReadOnly]
        private bool wasUsingMouse;

        
        // Start is called before the first frame update
        void Start()
        {
            // シーン中の UniWindowController を取得
            uniwinc = GameObject.FindObjectOfType<UniWindowController>();
            if (uniwinc) isHitTestEnabled = uniwinc.isHitTestEnabled;
        }

        // Update is called once per frame
        void Update()
        {
            DragMove();
        }

        /// <summary>
        /// ドラッグ開始時の処理
        /// </summary>
        void BeginDragging()
        {
            dragStartedPosition = Input.mousePosition;
            if (!_isDragging)
            {
                // ドラッグ中はヒットテストを無効にする
                isHitTestEnabled = uniwinc.isHitTestEnabled;
                uniwinc.isHitTestEnabled = false;
                uniwinc.isClickThrough = false;
            }
            _isDragging = true;
        }

        /// <summary>
        /// ドラッグ終了時の処理
        /// </summary>
        void EndDragging()
        {
            if (_isDragging)
            {
                uniwinc.isHitTestEnabled = isHitTestEnabled; 
            }
            activeFingerId = -1;
            _isDragging = false;
        }
        
        /// <summary>
        /// 最大化時以外なら、マウスドラッグによってウィンドウを移動
        /// </summary>
        void DragMove()
        {
            if (!uniwinc) return;

            // ドラッグでの移動が無効化されていた場合
            if (!enabled)
            {
                EndDragging();
                return;
            }

            // 最大化状態ならウィンドウ移動は行わないようにする
            bool isFullScreen = false;

            // フルスクリーンならウィンドウ移動は行わない
#if !UNITY_EDITOR
            //  エディタだと true になってしまうようなので、エディタ以外でのみ確認
            if (Screen.fullScreen) isFullScreen = true;
#endif
            if (isFullScreen)
            {
                EndDragging();
                return;
            }

            // マウス左ボタンでドラッグ開始判定
            if (Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
            {
                wasUsingMouse = true;
                BeginDragging();
            }

            int targetTouchIndex = -1;
            if (activeFingerId < 0)
            {
                // まだ追跡中の指が無かった場合、Beganとなるタッチがあればそれを追跡候補に挙げる
                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (Input.GetTouch(i).phase == TouchPhase.Began)
                    {
                        firstTouch = Input.GetTouch(i);     // まだドラッグ開始とはせず、透過画素判定に回す。
                        break;
                    }
                }
            }
            else
            {
                // 追跡中の指がある場合
                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (activeFingerId == Input.GetTouch(i).fingerId)
                    {
                        targetTouchIndex = i;
                        break;
                    }
                }
            }

            // タッチによるドラッグ開始の判定
            if (targetTouchIndex >= 0 && !_isDragging)
            {
                dragStartedPosition = Input.GetTouch(targetTouchIndex).position;
                wasUsingMouse = false;
                BeginDragging();
            }

            // ドラッグ終了の判定
            if (wasUsingMouse && !Input.GetMouseButton(0))
            {
                EndDragging();
            }
            else if (!wasUsingMouse && targetTouchIndex < 0)
            {
                EndDragging();
            }

            // ドラッグ中ならば、ウィンドウ位置を更新
            if (_isDragging)
            {
                Vector2 delta;
                if (wasUsingMouse)
                {
                    //  マウスによるドラッグ時
                    Vector2 mousePos = Input.mousePosition;
                    delta = mousePos - dragStartedPosition;
                }
                else
                {
                    // タッチによるドラッグ時
                    Touch touch = Input.GetTouch(targetTouchIndex);
                    delta = touch.position - dragStartedPosition;
                }
                Vector2 position = uniwinc.windowPosition;  // 現在のウィンドウ位置を取得
                position += delta; // ウィンドウ位置に上下左右移動分を加える
                uniwinc.windowPosition = position;   // ウィンドウ位置を設定
            }
        }
    }
}
