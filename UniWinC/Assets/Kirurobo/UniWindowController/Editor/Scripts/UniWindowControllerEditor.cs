/*
 * UniWindowControllerEditor.cs
 *
 * Author: Kirurobo http://x.com/kirurobo
 * License: MIT
 */

//  Assembry Definition を有効にしてから、ビルド時に Editor クラスがないとエラーが出る。
//   そこで丸ごと UNITY_EDITOR が無い場合は無視するものとした
#if UNITY_EDITOR

using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEngine.Rendering;

namespace Kirurobo
{
    /// <summary>
    /// UniWindowControllerのためのエディタカスタマイズ部分
    /// </summary>
    [CustomEditor(typeof(UniWindowController))]
    public class UniWindowControllerEditor : Editor
    {
        /// <summary>
        /// カーソル下の色を表示するためのプロパティ
        /// </summary>
        SerializedProperty pickedColor;
        
        /// <summary>
        /// ゲームビューのウィンドウ
        /// </summary>
        private EditorWindow gameViewWindow;

        /// <summary>
        /// プロジェクト設定に関する警告を閉じておくか
        private bool isWarningDismissed = false;

        /// <summary>
        /// URP に関する警告を閉じておくか
        /// </summary>
        private bool isUrpWarningDismissed = true;

        /// <summary>
        /// URP が有効かどうか
        /// </summary>
        private bool hasUrp = false;

        void OnEnable()
        {
            LoadSettings();

            pickedColor = serializedObject.FindProperty("pickedColor");

            // URP が有効か否かを判定
            hasUrp = GetUrpSettings();
        }

        void OnDisable()
        {
            SaveSettings();
        }

        /// <summary>
        /// URPが有効か否かを検出
        /// </summary>
        /// <returns></returns>
        private bool GetUrpSettings()
        {
            var renderPipelineAsset = GraphicsSettings.defaultRenderPipeline;
            if (renderPipelineAsset == null || renderPipelineAsset.GetType().Name != "UniversalRenderPipelineAsset")
            {
                // URP が設定されていない
                return false;
            }
            return true;
        }

        private void LoadSettings()
        {
            isWarningDismissed = EditorUserSettings.GetConfigValue("WindowController_IS_WARNING DISMISSED") == "1";
        }

        private void SaveSettings()
        {
            EditorUserSettings.SetConfigValue("WindowController_IS_WARNING DISMISSED", isWarningDismissed ? "1" : "0");
        }

        /// <summary>
        /// インスペクタでの表示をカスタマイズ
        /// </summary>
        /// <description>
        /// 参考情報および、推奨設定の変更欄を表示します。
        /// </description>
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

            // Project Settings の推奨設定を表示
            isWarningDismissed = ShowPlayerSettingsValidation(isWarningDismissed);

            // URP 関連の推奨設定を表示
            isUrpWarningDismissed = ShowUrpSettingsValidation(isUrpWarningDismissed);
        }

        /// <summary>
        /// Project Settings に関する推奨設定の自動設定欄を表示
        /// </summary>
        private bool ShowPlayerSettingsValidation(bool dismissed) {
            // 以下は Project Settings 関連
            EditorGUILayout.Space();

            bool enableValidation = EditorGUILayout.Foldout(!dismissed, "Player Settings validation");

            // チェックするかどうかを記憶
            if (enableValidation == dismissed)
            {
                dismissed = !enableValidation;
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
                        "✘ Dismiss this validation",
                        GUILayout.MinHeight(25f)
                        ))
                    {
                        dismissed = true;
                        
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
            return dismissed;
        }

        /// <summary>
        /// URP に関する推奨設定の自動設定欄を表示
        /// </summary>
        private bool ShowUrpSettingsValidation(bool dismissed) {
            // URP が無効ならば何もしない
            if (!hasUrp) return dismissed;

            // 以下は URP 関連の自動設定
            EditorGUILayout.Space();

            bool enableValidation = EditorGUILayout.Foldout(!dismissed, "URP Settings validation");
            // チェックするかどうかを記憶
            if (enableValidation == dismissed)
            {
                dismissed = !enableValidation;
            }
            // 推奨設定のチェック
            //if (!isWarningDismissed)
            if (enableValidation)
            {
                if (ValidateUrpSettings(false))
                {
                    // Apply all recommendation
                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button(
                        "✔ Fix all settings to recommended values",
                        GUILayout.MinHeight(25f)
                        ))
                    {
                        ValidateUrpSettings(true);
                    }

                    // Dismiss the validation
                    GUI.backgroundColor = Color.yellow;
                    if (GUILayout.Button(
                        "✘ Dismiss this validation",
                        GUILayout.MinHeight(25f)
                        ))
                    {
                        dismissed = true;
                    }
                    
                    EditorGUILayout.Space();
                }
                else
                {
                    GUI.color = Color.green;
                    GUILayout.Label("OK!");
                }
                
                EditorGUILayout.Space();
            }
            return dismissed;
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
        /// Show the recommendation only
        /// </summary>
        /// <param name="message">Warning message</param>
        private void ShowInfo(string message, Object target = null)

        {
            // Show the message and a fix button
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox(message, MessageType.Info, true);
            GUILayout.FlexibleSpace();

            // 自動設定できない対象は、プロジェクトウィンドウで示すのみ
            if (target != null)
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.Space();
                if (GUILayout.Button("Ping", GUILayout.Width(60f))) { EditorGUIUtility.PingObject(target); }
                //GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.EndHorizontal();
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

            // Direct3D12 は透過ウィンドウに対応していないので、Graphics APIs for Windows から除外することを推奨
            if (PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows))
            {
                // 自動の場合も警告を出す
                ShowInfo(
                    "Direct3D12 is not supported for transparent window. " +
                    "Please consider using Direct3D11 instead of the 'Auto Graphics API for Windows' setting in Player Settings.",
                    null
                );
            }
            else if (PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneWindows).Contains(GraphicsDeviceType.Direct3D12))
            {
                // Graphhics APIs for Windows に Direct3D12 が含まれている場合は警告を出す
                ShowInfo(
                    "Direct3D12 is not supported for transparent window. " +
                    "Please remove Direct3D12 from 'Graphics APIs for Windows' in Player Settings.",
                    null
                );
            }
#endif

            return invalid;
        }
        
        /// <summary>
        /// Validate player settings
        /// </summary>
        /// <param name="silentFix">false: show warning and fix button, true: fix without showing</param>
        /// <returns>true if there are any invalid items</returns>
        private bool ValidateUrpSettings(bool silentFix = false)
        {
            bool invalid = false;

            // Universal Render Pipelineが有効ならば、HDRの無効化を推奨
            foreach (var cam in Camera.allCameras)
            {
                if (cam.allowHDR) {
                    string name = cam.name;
                    invalid = true;
                    FixSetting(
                        $"{name}: Disable 'HDR' in the camera to make the window transparent.",
                        () => cam.allowHDR = false,
                        silentFix
                    );
                }
                if (cam.allowMSAA) {
                    string name = cam.name;
                    invalid = true;
                    FixSetting(
                        $"{name}: Disable 'MSAA' in the camera to make the window transparent.",
                        () => cam.allowMSAA = false,
                        silentFix
                    );
                }
            }

            var urpAsset = GraphicsSettings.defaultRenderPipeline;
            if (hasUrp && urpAsset != null)
            {
                // hasUrp == true の時点で urpAsset は UniversalRenderPipelineAsset であるはず。そのため allowPostProcessAlphaOutput があるはず
                var alphaProcessingProperty = urpAsset.GetType().GetProperty("allowPostProcessAlphaOutput", BindingFlags.Public | BindingFlags.Instance);
                if (alphaProcessingProperty != null)
                {
                    var alphaProcessing = alphaProcessingProperty.GetValue(urpAsset);
                    if (!(bool)alphaProcessing)
                    {
                        invalid = true;
                        ShowInfo(
                            "Turn on 'Alpha Processing' in the URP asset",
                            urpAsset
                        );
                    }
                }
            }

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