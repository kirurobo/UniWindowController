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
            
            // 事前にエディタから設定したビルド設定を利用する
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
                locationPathName = EditorUserBuildSettings.GetBuildLocation(buildTarget),
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
