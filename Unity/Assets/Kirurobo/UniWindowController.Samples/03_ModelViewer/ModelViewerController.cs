using System.Collections;
using System.Collections.Generic;
using Kirurobo;
using UnityEngine;

public class ModelViewerController : MonoBehaviour
{
    private UniWindowController uniwin;
    
    // Start is called before the first frame update
    void Start()
    {
        //UniWinCore.DebugMonitorInfo();

        uniwin = GameObject.FindObjectOfType<UniWindowController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            uniwin.FitToMonitor(0);
        }
    }
}
