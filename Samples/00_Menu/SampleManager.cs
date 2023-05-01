using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Kirurobo
{
    public class SampleManager : MonoBehaviour
    {
        static readonly string[] _scenes =
        {
        "SampleMenu",
        "SimpleSample",
        "UiSample",
        "FullScreenSample",
        "FileDialogSample"
    };

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

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            UniWindowController.current.SetCamera(Camera.main);
        }

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

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

        //private void OnGUI()
        //{
        //    float y = 0;

        //    for (int index = 0; index < _scenes.Length; index++)
        //    {
        //        if (GUI.Button(new Rect(0, y, 160, 36), _scenes[index]))
        //        {
        //            //if (index == 1)
        //            //{
        //            //    // SimpleSample の場合はスクリプトでの制御がないため、ここで透明化
        //            //    UniWindowController.current.isTransparent = true;
        //            //}
        //            //else if (index == 3)
        //            //{
        //            //    // FullScreenSample の場合は強制的に最大化
        //            //    UniWindowController.current.shouldFitMonitor = true;
        //            //}

        //            //SceneManager.LoadScene(_scenes[index]);
        //        }
        //        y += 40;
        //    }
        //}
    }
}
