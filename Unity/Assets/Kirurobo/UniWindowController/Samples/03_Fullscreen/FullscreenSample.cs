using System.Collections;
using System.Collections.Generic;
using Kirurobo;
using UnityEngine;

namespace Kirurobo
{
    public class FullscreenSample : MonoBehaviour
    {
        private UniWindowController uniwin;

        // Start is called before the first frame update
        void Start()
        {
            //UniWinCore.DebugMonitorInfo();

            uniwin = GameObject.FindObjectOfType<UniWindowController>();

            uniwin.OnStateChanged += () => { Debug.Log("State changed"); };
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}
