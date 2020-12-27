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
        public enum ZoomType : int
        {
            None = 0,
            Dolly = 1, // Dolly in/out
            Zoom = 2, // Zoom in/out
        }

        [Flags]
        public enum MouseButton : int
        {
            Left = 0,
            Right = 1,
            Middle = 2,
        }
        
        public RotationAxes axes = RotationAxes.PitchAndYaw;
        [FormerlySerializedAs("zoomMode")] public ZoomType zoomType = ZoomType.Dolly;
        public float sensitivityX = 15f;
        public float sensitivityY = 15f;
        public float dragSensitivity = 0.1f;
        public float wheelSensitivity = 0.5f;

        public Vector2 minimumAngles = new Vector2(-90f, -360f);
        public Vector2 maximumAngles = new Vector2(90f, 360f);

        [Tooltip("None means to set the parent transform")]
        public Transform centerTransform; // 回転中心

        internal GameObject centerObject = null; // 回転中心Transformが指定されなかった場合に作成される

        [SerializeField]
        internal Vector3 rotation;
        internal Vector3 translation;
        internal Vector3 lastMousePosition;    // 直前フレームでのマウス座標 

        internal Vector3 relativePosition;
        internal Quaternion relativeRotation;
        internal Vector3 originalLocalScale;
        internal float dolly;
        internal float zoom;

        public Camera currentCamera;

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
            translation = Vector3.zero;
            dolly = 0f;
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
            if (Input.GetMouseButton(1) && IsHit())
            {
                // 右ボタンドラッグで回転
                if ((axes & RotationAxes.Yaw) > RotationAxes.None)
                {
                    rotation.y += Input.GetAxis("Mouse X") * sensitivityX;
                    rotation.y = ClampAngle(rotation.y, minimumAngles.y, maximumAngles.y);
                }

                if ((axes & RotationAxes.Pitch) > RotationAxes.None)
                {
                    rotation.x += Input.GetAxis("Mouse Y") * sensitivityY;
                    rotation.x = ClampAngle(rotation.x, minimumAngles.x, maximumAngles.x);
                }

                UpdateTransform();
            }
            else if (Input.GetMouseButton(0) && IsHit())
            {
                Vector3 relativePos = Quaternion.Inverse(currentCamera.transform.rotation) * (transform.position - currentCamera.transform.position);
                Vector3 screenPos = currentCamera.WorldToScreenPoint(transform.position);
                Vector3 mousePos = Input.mousePosition - lastMousePosition; 
                mousePos.z = 0f;
                Vector3 delta = currentCamera.ScreenToWorldPoint(mousePos + screenPos);
                //Debug.Log(mousePos);
                translation = delta - centerTransform.position;
                UpdateTransform();
            }
            else if (!Mathf.Approximately(Input.GetAxis("Mouse ScrollWheel"), 0f) && IsHit())
            {
                // ホイールで接近・離脱
                float wheelDelta = Input.GetAxis("Mouse ScrollWheel") * wheelSensitivity;

                ZoomType type = zoomType;

                // Shiftキーが押されていて、かつZoomModeがZoomかDollyならば、モードを入れ替える
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    if (type == ZoomType.Dolly)
                    {
                        type = ZoomType.Zoom;
                    }
                    else if (type == ZoomType.Zoom)
                    {
                        type = ZoomType.Dolly;
                    }
                }

                if (wheelDelta != 0f)
                {
                    if ((type & ZoomType.Dolly) != ZoomType.None)
                    {
                        // ドリーの場合。カメラを近づけたり遠ざけたり。
                        dolly += wheelDelta;
                        dolly = Mathf.Clamp(dolly, -2f, 5f); // Logarithm of distance [m] range

                        UpdateTransform();
                    }
                    else if ((type & ZoomType.Zoom) != ZoomType.None)
                    {
                        // ズームの場合。カメラのFOVを変更
                        zoom -= wheelDelta;
                        zoom = Mathf.Clamp(zoom, -1f, 2f); // Logarithm of field-of-view [deg] range

                        UpdateTransform();
                    }
                }
            }

            lastMousePosition = Input.mousePosition;
        }

        /// <summary>
        /// マウスでの操作時、オブジェクトにヒットしたか判定
        /// </summary>
        /// <returns></returns>
        internal bool IsHit()
        {
            RaycastHit hit;
            Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);

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