using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
// ReSharper disable UnusedMember.Local

namespace Kirurobo
{
    class UniWindowControllerBatch
    {
        //[MenuItem("Build/Build OSX")]
        static void PerformBuild()
        {
            // コマンドライン引数の最後が出力パスだとする
            //string outputPath = System.Environment.GetCommandLineArgs().Last();

            // var buildPlayerOptions = new BuildPlayerOptions();
            // buildPlayerOptions.scenes = sceneList.ToArray();
            // buildPlayerOptions.locationPathName = outputPath;
            // buildPlayerOptions.target = BuildTarget.StandaloneOSX;
            // buildPlayerOptions.options = BuildOptions.None;

            // 事前にエディタから設定したビルド設定を利用
            var scenes = EditorBuildSettings.scenes;
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var locationPath = EditorUserBuildSettings.GetBuildLocation(buildTarget);

            // ビルド対象は環境に合わせて上書き
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            buildTarget = BuildTarget.StandaloneOSX;
            locationPath = "Builds/macOS/" + Application.productName;
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            buildTarget = BuildTarget.StandaloneWindows64;
            locationPath = "Builds/Win64/" + Application.productName;
#endif

            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = EditorBuildSettingsScene.GetActiveSceneList(scenes),
                locationPathName = locationPath,
                target = buildTarget,
                options = BuildOptions.None
            };

            // // 内容チェック用
            // foreach (var scene in buildPlayerOptions.scenes)
            // {
            //     Debug.Log(scene);
            // }
            // Debug.Log(buildPlayerOptions.locationPathName);
            // return;

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            var summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded");
            } else if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Build failed");
                //EditorApplication.Exit(1);
                throw new BuildFailedException(report.summary.ToString());
            }
        }
    }
}
