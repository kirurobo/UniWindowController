/*
 * ModelController
 *
 * Rotate, translate and scale the object
 * 
 * Author: Kirurobo http://twitter.com/kirurobo
 * License: MIT
 */

using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kirurobo
{
    public class ModelController : MonoBehaviour
    {
        [Flags]
        public enum RotationAxes : int
        {
            None = 0,
            Pitch = 1,
            Yaw = 2,
            PitchAndYaw = 3
        }

        [Flags]
        public enum DragState
        {
            None,
            Rotating,
            Translating,
        }
        
        public RotationAxes axes = RotationAxes.PitchAndYaw;
        public float yawSensitivity = 1f;
        public float pitchSensitvity = 1f;
        public float scaleSensitivity = 0.5f;

        public Vector2 minimumAngles = new Vector2(-90f, -360f);
        public Vector2 maximumAngles = new Vector2(90f, 360f);

        [Tooltip("Restrict to move out from screen")]
        public bool confineTranslation = true;        // 並進移動をウィンドウ（Screen）の範囲に制限するか

        [Tooltip("Default is the parent transform")]
        public Transform centerTransform; // 回転中心
        
        [Tooltip("Default is the main camera")]
        public Camera currentCamera;
        
        internal GameObject centerObject = null; // 回転中心Transformが指定されなかった場合に作成される

        internal Vector3 rotation;
        internal Vector3 translation;
        internal Vector3 lastMousePosition;    // 直前フレームでのマウス座標 
        internal DragState dragState;            // ドラッグ中は開始時のボタンに合わせた内容にする

        internal Vector3 relativePosition;
        internal Quaternion relativeRotation;
        internal Vector3 originalLocalScale;
        internal float zoom;


        void Start()
        {
            Initialize();
            SetupTransform();
        }

        void OnDestroy()
        {
            // 回転中心を独自に作成していれば、削除
            if (centerObject) GameObject.Destroy(centerObject);
        }

        void Update()
        {
            if (!currentCamera.isActiveAndEnabled) return;
            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
            {
                HandleMouse();
            }
        }

        /// <summary>
        /// 必要なオブジェクトを取得・準備
        /// </summary>
        internal void Initialize()
        {
            if (!centerTransform)
            {
                centerTransform = this.transform.parent;
                if (!centerTransform || centerTransform == this.transform)
                {
                    centerObject = new GameObject();
                    centerTransform = centerObject.transform;
                    centerTransform.position = Vector3.zero;
                }
            }

            if (!currentCamera)
            {
                currentCamera = Camera.main;
            }

            lastMousePosition = Input.mousePosition;
        }

        /// <summary>
        /// 初期位置・姿勢の設定
        /// 対象となるオブジェクトがそろった後で実行する
        /// </summary>
        internal void SetupTransform()
        {
            relativePosition = transform.position- centerTransform.position; // オブジェクトから中心座標へのベクトル
            relativeRotation = transform.rotation * Quaternion.Inverse(centerTransform.rotation);
            originalLocalScale = transform.localScale;

            ResetTransform();
        }

        /// <summary>
        /// Reset rotation and translation.
        /// </summary>
        public void ResetTransform()
        {
            rotation = relativeRotation.eulerAngles;
            translation = relativePosition;
            zoom = 0f;

            UpdateTransform();
        }

        /// <summary>
        /// Apply rotation and translation
        /// </summary>
        internal void UpdateTransform()
        {
            Quaternion rot = Quaternion.Euler(rotation);
            transform.rotation = rot;
            transform.position = centerTransform.position + translation;

            transform.localScale = originalLocalScale * Mathf.Pow(10f, zoom);
        }

        internal virtual void HandleMouse()
        {
            Vector3 mousePos = Input.mousePosition;
            
            if (Input.GetMouseButtonDown(0))
            {
                // 左ボタン(0)ドラッグでは並進移動を行う
                if (dragState == DragState.None && IsHit(mousePos))
                {
                    dragState = DragState.Translating;
                    
                    // 画面範囲に制限する
                    if (confineTranslation)
                    {
                        Vector3 screenMax = new Vector3(Screen.width, Screen.height);
                        mousePos = Vector3.Max(Vector3.Min(mousePos, screenMax), Vector3.zero);
                    }
                    
                    lastMousePosition = mousePos;        // ドラッグ開始時にはリセット
                } 
            }
            else if (Input.GetMouseButtonDown(1))
            {
                // 右ボタン(1)ドラッグでは回転を行う
                if (dragState == DragState.None && IsHit(mousePos))
                {
                    dragState = DragState.Rotating;
                    lastMousePosition = mousePos;        // ドラッグ開始時にはリセット
                } 
            }

            // ドラッグで回転
            if (dragState == DragState.Rotating)
            {
                // ボタンが押されている間のみ操作
                if (Input.GetMouseButton(1))
                {
                    // ドラッグで回転
                    if ((axes & RotationAxes.Yaw) > RotationAxes.None)
                    {
                        rotation.y -= (mousePos.x - lastMousePosition.x) * 360f / Screen.width * yawSensitivity;
                        rotation.y = ClampAngle(rotation.y, minimumAngles.y, maximumAngles.y);
                    }

                    if ((axes & RotationAxes.Pitch) > RotationAxes.None)
                    {
                        rotation.x += (mousePos.y - lastMousePosition.y) * 360f / Screen.height * pitchSensitvity;
                        rotation.x = ClampAngle(rotation.x, minimumAngles.x, maximumAngles.x);
                    }

                    UpdateTransform();
                }
                else
                {
                    // 右ボタンが離されていれば回転は終了
                    dragState = DragState.None;
                }
            }
            
            // ドラッグで並進移動
            if (dragState == DragState.Translating)
            {
                // ボタンが押されている間のみ操作
                if (Input.GetMouseButton(0))
                {
                    // 画面範囲に制限する
                    if (confineTranslation)
                    {
                        Vector3 screenMax = new Vector3(Screen.width, Screen.height);
                        mousePos = Vector3.Max(Vector3.Min(mousePos, screenMax), Vector3.zero);
                    }
                    
                    Vector3 screenPos = currentCamera.WorldToScreenPoint(transform.position);
                    Vector3 deltaPos = mousePos - lastMousePosition; 
                    deltaPos.z = 0f;
                    Vector3 worldPos = currentCamera.ScreenToWorldPoint(screenPos + deltaPos);
                    translation = worldPos - centerTransform.position;
                    
                    UpdateTransform();
                }
                else
                {
                    // ボタンが離されていれば並進は終了
                    dragState = DragState.None;
                }
            }
            
            // ホイールが回転されれば、拡大縮小
            if (!Mathf.Approximately(Input.GetAxis("Mouse ScrollWheel"), 0f) && IsHit(mousePos))
            {
                // ホイールによる操作量
                float wheelDelta = Input.GetAxis("Mouse ScrollWheel") * scaleSensitivity;

                // 倍率を変更
                zoom -= wheelDelta;
                zoom = Mathf.Clamp(zoom, -1f, 2f); // Logarithm of field-of-view [deg] range

                UpdateTransform();
            }
                    
            lastMousePosition = mousePos;
        }

        /// <summary>
        /// マウスでの操作時、オブジェクトにヒットしたか判定
        /// </summary>
        /// <returns></returns>
        internal bool IsHit(Vector3 screenPosition)
        {
            RaycastHit hit;
            Ray ray = currentCamera.ScreenPointToRay(screenPosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.IsChildOf(transform)) return true;
            }

            return false;
        }

        /// <summary>
        /// 指定範囲から外れる角度の場合、補正する
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -min) angle = -((-angle) % 360f);
            if (angle > max) angle = angle % 360f;
            return Mathf.Clamp(angle, min, max);
        }
    }
}