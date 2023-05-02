using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Kirurobo
{
    public class SampleManager : MonoBehaviour
    {
        private static SampleManager _instance;
        public static SampleManager Instance => _instance ?? (_instance = GameObject.FindObjectOfType<SampleManager>() ?? new SampleManager());

        public Canvas canvas;

        private void Awake()
        {
            // シングルトンとする。既にインスタンスがあれば自分を破棄
            if (this != Instance)
            {
                Destroy(this.gameObject);
                return;
            }

            DontDestroyOnLoad(Instance);
            DontDestroyOnLoad(UniWindowController.current);

            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        /// <summary>
        /// シーンロード時にメインカメラを記憶
        /// </summary>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            UniWindowController.current.SetCamera(Camera.main);
        }

        /// <summary>
        /// 指定の名前のシーンを開く
        /// </summary>
        /// <param name="name">シーン名</param>
        public void LoadScene(string name)
        {
            if (name == "SimpleSample")
            {
                // SimpleSample の場合はスクリプトでの制御がないため、ここで透明化
                UniWindowController.current.isTransparent = true;
            }
            else if (name == "FullScreenSample")
            {
                // FullScreenSample の場合は強制的に最大化
                UniWindowController.current.shouldFitMonitor = true;
            }

            SceneManager.LoadScene(name);
        }

        /// <summary>
        /// 終了
        /// </summary>
        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
