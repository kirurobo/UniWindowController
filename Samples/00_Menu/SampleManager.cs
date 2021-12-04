using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SampleManager : MonoBehaviour
{
    static readonly string[] _scenes =
    {
        "SimpleSample",
        "UiSample",
        "FullScreenSample",
        "FileDialogSample"
    };

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CallNextScene(int index)
    {
        if ((index > 0) && (index <= _scenes.Length))
        {
            SceneManager.LoadScene(_scenes[index - 1]);
        }
    }
}
