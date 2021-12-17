/*
 * UniWindowControllerEditor.cs
 * 
 * Author: Kirurobo http://twitter.com/kirurobo
 * License: MIT
 */

//  Assembry Definition を有効にしてから、ビルド時に Editor クラスがないとエラーが出る。
//   そこで丸ごと UNITY_EDITOR が無い場合は無視するものとした
#if UNITY_EDITOR

using System.Linq;
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
                if (ValidateSettings(false))
                {
                    // Apply all recommendation
                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button(
                        "✔ Fix all settings to recommended values",
                        GUILayout.MinHeight(25f)
                        ))
                    {
                        ValidateSettings(true);
                    }

                    // Dismiss the validation
                    GUI.backgroundColor = Color.yellow;
                    if (GUILayout.Button(
                        "✘ Mute this validation",
                        GUILayout.MinHeight(25f)
                        ))
                    {
                        isWarningDismissed = true;
                        
                        //SaveSettings();        // Uncomment this if save you want to save immediately
                    }
                    
                    EditorGUILayout.Space();
                }
                else
                {
                    GUI.color = Color.green;
                    GUILayout.Label("OK!");
                }
                
                // Open the player settings page
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUI.color = Color.white;
                GUI.backgroundColor = Color.white;
                if (GUILayout.Button(
                    "Open Player Settings",
                    GUILayout.MinHeight(25f), GUILayout.Width(200f)
                ))
                {
                    SettingsService.OpenProjectSettings("Project/Player");
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space();
            }
        }


        private delegate void FixMethod();

        /// <summary>
        /// Show or fix the setting
        /// </summary>
        /// <param name="message">Warning message</param>
        /// <param name="fixAction"></param>
        /// <param name="silentFix">false: show warning and fix button, true: fix without showing</param>
        private void FixSetting(string message, FixMethod fixAction, bool silentFix = false)

        {
            if (silentFix)
            {
                // Fix
                fixAction.Invoke();
            }
            else
            {
                // Show the message and a fix button
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox(message, MessageType.Warning, true);
                GUILayout.FlexibleSpace();
                
                EditorGUILayout.BeginVertical();
                EditorGUILayout.Space();
                if (GUILayout.Button("Fix", GUILayout.Width(60f))) { fixAction.Invoke(); }
                //GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.EndHorizontal();
            }
        }
        
        /// <summary>
        /// Validate player settings
        /// </summary>
        /// <param name="silentFix">false: show warning and fix button, true: fix without showing</param>
        /// <returns>true if there are any invalid items</returns>
        private bool ValidateSettings(bool silentFix = false)
        {
            bool invalid = false;
            
            // バックグラウンドでも実行する。クリックスルー切替などで必要
            if (!PlayerSettings.runInBackground)
            {
                invalid = true;
                FixSetting(
                    "'Run in background' is highly recommended.",
                    () => PlayerSettings.runInBackground = true,
                    silentFix
                    );
            }

            // サイズ変更可能なウィンドウとする。必須ではないがウィンドウ枠無効時にサイズも変わるので変更可能である方が自然
            if (!PlayerSettings.resizableWindow)
            {
                invalid = true;
                FixSetting(
                    "'Resizable window' is recommended.",
                    () => PlayerSettings.resizableWindow = true,
                    silentFix
                );
            }

            // フルスクリーンでなくウィンドウとする
#if UNITY_2018_1_OR_NEWER
            // Unity 2018 からはフルスクリーン指定の仕様が変わった
            if (PlayerSettings.fullScreenMode != FullScreenMode.Windowed)
            {
                invalid = true;
                FixSetting(
                    "Chose 'Windowed' in 'Fullscreen Mode'.", 
                    () => PlayerSettings.fullScreenMode = FullScreenMode.Windowed,
                    silentFix
                );

            }
#else
            if (PlayerSettings.defaultIsFullScreen)
            {
                invalid = true;
                FixSetting(
                    "'Default is full screen' is not recommended.", 
                    () => PlayerSettings.defaultIsFullScreen = false,
                    silentFix
                );
            }
#endif

            // フルスクリーンとウィンドウの切替を無効とする
            if (PlayerSettings.allowFullscreenSwitch)
            {
                invalid = true;
                FixSetting(
                    "Disallow fullscreen switch.", 
                    () => PlayerSettings.allowFullscreenSwitch = false,
                    silentFix
                );
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
                invalid = true;
                FixSetting(
                    "Disable 'Use DXGI Flip Mode Swapchain' to make the window transparent.",
                    () => PlayerSettings.useFlipModelSwapchain = false,
                    silentFix
                );
            }
#endif

            return invalid;
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
    /// Set a bool property editable
    /// Reference: http://ponkotsu-hiyorin.hateblo.jp/entry/2015/10/20/003042
    /// Reference: https://forum.unity.com/threads/c-class-property-with-reflection-in-propertydrawer-not-saving-to-prefab.473942/
    /// </summary>
    [CustomPropertyDrawer(typeof(EditablePropertyAttribute))]
    public class UniWindowControllerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //base.OnGUI(position, property, label);

            Object obj = property.serializedObject.targetObject;
                
            // Range(min, max) が設定されていれば取得
            FieldInfo fieldInfo = obj.GetType().GetField(
                property.name,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static
            );
            var rangeAttrs = fieldInfo?.GetCustomAttributes(typeof(RangeAttribute), true) as RangeAttribute[];
            RangeAttribute range = (rangeAttrs?.Length > 0 ? rangeAttrs.First() : null);
                
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                // 変数の先頭が '_' であることが動作の条件
                if (property.name[0] == '_')
                {
                    string propertyName = property.name.Substring(1);       // '_' なしをプロパティ名として取得
                    PropertyInfo info = obj.GetType().GetProperty(propertyName);
                    MethodInfo getMethod = default(MethodInfo);
                    MethodInfo setMethod = default(MethodInfo);
                    if (info.CanRead) { getMethod = info.GetGetMethod(); }
                    if (info.CanWrite) { setMethod = info.GetSetMethod(); }
                    
                    if (property.type == "bool")
                    { var oldValue = property.boolValue;
                        if (getMethod != null)
                        {
                            oldValue = (bool)getMethod.Invoke(obj, null);
                        }
                        GUI.enabled = (setMethod != null);
                        EditorGUI.PropertyField(position, property, label, true);
                        GUI.enabled = true;
                        var newValue = property.boolValue;
                        if ((setMethod != null) && (oldValue != newValue))
                        {
                            setMethod.Invoke(obj, new[] { (object)newValue });
                        }
                    }
                    else if (property.type == "float")
                    {
                        
                        var oldValue = property.floatValue;
                        if (getMethod != null)
                        {
                            oldValue = (float) getMethod.Invoke(obj, null);
                        }

                        GUI.enabled = (setMethod != null);
                        if (range != null)
                        {
                            EditorGUI.Slider(position, property, range.min, range.max, label);
                        }
                        else
                        {
                            EditorGUI.PropertyField(position, property, label, true);
                        }
                        GUI.enabled = true;
                        
                        var newValue = property.floatValue;
                        if ((setMethod != null) && (oldValue != newValue))
                        {
                            setMethod.Invoke(obj, new[] {(object) newValue});
                        }
                    }
                    else
                    {
                        // bool, float 以外は今のところ非対応で Readonly とする
                        GUI.enabled = false;
                        EditorGUI.PropertyField(position, property, label, true);
                        GUI.enabled = true;
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
                // Range 指定があればスライダー
                if (range != null)
                {
                    EditorGUI.Slider(position, property, range.min, range.max, label);
                }
                else
                {
                    EditorGUI.PropertyField(position, property, label, true);
                }
            }

        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}
#endif