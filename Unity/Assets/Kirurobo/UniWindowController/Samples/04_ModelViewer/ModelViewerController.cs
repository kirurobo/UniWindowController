using System;
using System.Collections;
using System.Collections.Generic;
using Kirurobo;
using UnityEngine;

namespace Kirurobo
{
    public class ModelViewerController : MonoBehaviour
    {
        private UniWindowController uniwin;

        // Start is called before the first frame update
        void Start()
        {
            //UniWinCore.DebugMonitorInfo();

            uniwin = GameObject.FindObjectOfType<UniWindowController>();
            uniwin.OnAppCommand += (key) =>
            {
                if (key > 0)
                {
                    Debug.Log("AppCommand: " + key.ToString());
                }
            };
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}
