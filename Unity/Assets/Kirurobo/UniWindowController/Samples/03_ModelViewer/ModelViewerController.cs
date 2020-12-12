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
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}
