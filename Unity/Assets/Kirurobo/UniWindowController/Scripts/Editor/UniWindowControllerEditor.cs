/*
 * UniWindowControllerEditor.cs
 * 
 * Author: Kirurobo http://twitter.com/kirurobo
 * License: MIT
 */

//  Assembry Definition を有効にしてから、ビルド時に Editor クラスがないとエラーが出る。
//   そこで丸ごと UNITY_EDITOR が無い場合は無視するものとした
#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Kirurobo
{
    /// <summary>
    /// UniWindowControllerのためのエディタカスタマイズ部分
    /// </summary>
    [CustomEditor(typeof(UniWindowController))]
    public class UniWindowControllerEditor : Editor
    {
        SerializedProperty pickedColor;
        
        private EditorWindow gameViewWindow;

        private bool isWarningDismissed = false;

        void OnEnable()
        {
            LoadSettings();

            pickedColor = serializedObject.FindProperty("pickedColor");
        }

        void OnDisable()
        {
            SaveSettings();
        }

        private void LoadSettings()
        {
            isWarningDismissed = EditorUserSettings.GetConfigValue("WindowController_IS_WARNING DISMISSED") == "1";
        }

        private void SaveSettings()
        {
            EditorUserSettings.SetConfigValue("WindowController_IS_WARNING DISMISSED", isWarningDismissed ? "1" : "0");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // カーソル下の色が得られていれば、その透明度を参考として表示
            if (pickedColor != null)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.LabelField("Picked Alpha", pickedColor.colorValue.a.ToString("P0"));
                EditorGUI.EndDisabledGroup();
            }

            // 以下は Project Settings 関連
            EditorGUILayout.Space();

            bool enableValidation = EditorGUILayout.Foldout(!isWarningDismissed, "Player Settings validation");

            // チェックするかどうかを記憶
            if (enableValidation == isWarningDismissed)
            {
                isWarningDismissed = !enableValidation;
            }

            // 推奨設定のチェック
            //if (!isWarningDismissed)
            if (enableValidation)
            {
                // Player Settings をチェックし、非推奨があれば警告メッセージを得る
                string[] warnings = ValidatePlayerSettings();

                //  チェックに引っかかればボタンを表示
                if (warnings.Length > 0)
                {

                    // 枠を作成
                    //EditorGUILayout.BeginVertical(GUI.skin.box);
                    //GUILayout.Label("Player Settings validation");

                    // 警告メッセージを表示
                    foreach (var message in warnings)
                    {
                        EditorGUILayout.HelpBox(message, MessageType.Warning);
                    }

                    // 推奨設定をすべて適用するボタン
                    GUI.backgroundColor = Color.green;
                    if (
                        GUILayout.Button(
                            "✔ Apply all recommended settings",
                            GUILayout.MinHeight(20f)
                        ))
                    {
                        ApplyRecommendedSettings();
                    }

                    // チェックを今後無視するボタン
                    GUI.backgroundColor = Color.yellow;
                    if (
                        GUILayout.Button(
                            "✘ Mute this validation",
                            GUILayout.MinHeight(20f)
                        ))
                    {
                        isWarningDismissed = true;
                        //SaveSettings();
                    }

                    //EditorGUILayout.EndVertical();
                }
                else
                {
                    GUI.color = Color.green;
                    GUILayout.Label("OK!");
                }
            }
        }

        /// <summary>
        /// Player設定を確認し、推奨設定になっていない項目のメッセージ一覧を得る
        /// </summary>
        /// <returns></returns>
        private string[] ValidatePlayerSettings()
        {
            // 警告メッセージのリスト
            List<string> warnings = new List<string>();

            // バックグラウンドでも実行する。クリックスルー切替などで必要
            if (!PlayerSettings.runInBackground)
            {
                warnings.Add("'Run in background' is highly recommended.");
            }

            // サイズ変更可能なウィンドウとする。必須ではないがウィンドウ枠無効時にサイズも変わるので変更可能である方が自然
            if (!PlayerSettings.resizableWindow)
            {
                warnings.Add("'Resizable window' is recommended.");
            }

            // フルスクリーンでなくウィンドウとする
#if UNITY_2018_1_OR_NEWER
            // Unity 2018 からはフルスクリーン指定の仕様が変わった
            if (PlayerSettings.fullScreenMode != FullScreenMode.Windowed)
            {
                warnings.Add("Chose 'Windowed' in 'Fullscreen Mode'.");
            }
#else
            if (PlayerSettings.defaultIsFullScreen)
            {
                warnings.Add("'Default is full screen' is not recommended.");
            }
#endif

            // フルスクリーンとウィンドウの切替を無効とする
            if (PlayerSettings.allowFullscreenSwitch)
            {
                warnings.Add("Disallow fullscreen switch.");
            }
            
            // Windowsでは Use DXGI Flip Mode Swapchain を無効にしないと透過できない
            // ↓Unity 2019.1.6未満だと useFlipModelSwapchain は無いはず
            //    なので除外のため書き連ねてあるが、ここまでサポートしなくて良い気もする。
#if UNITY_2019_1_6
#elif UNITY_2019_1_5
#elif UNITY_2019_1_4
#elif UNITY_2019_1_3
#elif UNITY_2019_1_2
#elif UNITY_2019_1_1
#elif UNITY_2019_1_0
#elif UNITY_2019_1_OR_NEWER
            // Unity 2019.1.7 以降であれば、Player 設定 の Use DXGI Flip... 無効化を推奨
            if (PlayerSettings.useFlipModelSwapchain)
            {
                warnings.Add("Disable 'Use DXGI Flip Mode Swapchain' to make the window transparent.");
            }
#endif

            return warnings.ToArray();
        }

        /// <summary>
        /// 推奨設定を一括で適用
        /// </summary>
        private void ApplyRecommendedSettings()
        {
#if UNITY_2018_1_OR_NEWER
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
#else
            PlayerSettings.defaultIsFullScreen = false;
#endif
            PlayerSettings.runInBackground = true;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.allowFullscreenSwitch = false;

#if UNITY_2019_1_6
#elif UNITY_2019_1_5
#elif UNITY_2019_1_4
#elif UNITY_2019_1_3
#elif UNITY_2019_1_2
#elif UNITY_2019_1_1
#elif UNITY_2019_1_0
#elif UNITY_2019_1_OR_NEWER
            PlayerSettings.useFlipModelSwapchain = false;
#endif

        }
    }

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class UniWindowControllerReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }

    /// <summary>
    /// Set to readonly during playing
    /// Reference: http://ponkotsu-hiyorin.hateblo.jp/entry/2015/10/20/003042
    /// Reference: https://forum.unity.com/threads/c-class-property-with-reflection-in-propertydrawer-not-saving-to-prefab.473942/
    /// </summary>
    [CustomPropertyDrawer(typeof(BoolPropertyAttribute))]
    public class UniWindowControllerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //base.OnGUI(position, property, label);

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if ((property.type == "bool") && (property.name[0] == '_'))
                {
                    Object obj = property.serializedObject.targetObject;
                    string propertyName = property.name.Substring(1);
                    PropertyInfo info = obj.GetType().GetProperty(propertyName);
                    MethodInfo getMethod = default(MethodInfo);
                    MethodInfo setMethod = default(MethodInfo);
                    if (info.CanRead) { getMethod = info.GetGetMethod(); }
                    if (info.CanWrite) { setMethod = info.GetSetMethod(); }

                    bool oldValue = property.boolValue;
                    if (getMethod != null)
                    {
                        oldValue = (bool)getMethod.Invoke(obj, null);
                    }
                    GUI.enabled = (setMethod != null);
                    EditorGUI.PropertyField(position, property, label, true);
                    GUI.enabled = true;
                    bool newValue = property.boolValue;
                    if ((setMethod != null) && (oldValue != newValue))
                    {
                        setMethod.Invoke(obj, new[] { (object)newValue });
                    }
                }
                else
                {
                    // Readonly
                    GUI.enabled = false;
                    EditorGUI.PropertyField(position, property, label, true);
                    GUI.enabled = true;
                }
            }
            else
            {
                // Default value
                EditorGUI.PropertyField(position, property, label, true);
            }

        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}
#endif