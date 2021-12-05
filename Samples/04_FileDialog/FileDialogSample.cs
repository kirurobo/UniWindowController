using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kirurobo
{
    /// <summary>
    /// Basic filepanel sample
    /// </summary>
    public class FileDialogSample : MonoBehaviour
    {
        string message = "";

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {

        }

        // Show buttons and a textarea
        private void OnGUI()
        {
            float x = 170f;

            if (GUI.Button(new Rect(x, 10, 160, 80), "Open a file"))
            {
                FilePanel.Settings settings = new FilePanel.Settings();
                settings.filters = new FilePanel.Filter[]
                {
                    new FilePanel.Filter("All files", "*"),
                    new FilePanel.Filter("Image files (*.png;*.jpg;*.jpeg;*.tiff;*.gif;*.tga)", "png", "jpg", "jpeg", "tiff", "gif", "tga"),
                    new FilePanel.Filter("Documents (*.txt;*.rtf;*.doc;*.docx)", "txt", "rtf", "doc", "docx"),
                };
                settings.title = "Open a file!";
                settings.initialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);

                message = "Canceled";
                FilePanel.OpenFilePanel(settings, (files) =>
                {
                    message = "Open a file\n" + string.Join("\n", files);
                });
            }

            if (GUI.Button(new Rect(x, 100, 160, 80), "Open multiple files"))
            {
                FilePanel.Settings settings = new FilePanel.Settings();
                settings.filters = new FilePanel.Filter[]
                {
                    new FilePanel.Filter("Image files (*.png;*.jpg;*.jpeg;*.tiff;*.gif;*.tga)", "png", "jpg", "jpeg", "tiff", "gif", "tga"),
                    new FilePanel.Filter("Documents (*.txt;*.rtf;*.doc;*.docx)", "txt", "rtf", "doc", "docx"),
                    new FilePanel.Filter("All files", "*"),
                };
                settings.flags = FilePanel.Flag.AllowMultipleSelection;
                settings.title = "Open multiple files!";
                settings.initialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);

                message = "Canceled";
                FilePanel.OpenFilePanel(settings, (files) =>
                {
                    message = "Open multiple files\n" + string.Join("\n", files);
                });
            }

            if (GUI.Button(new Rect(x, 200, 160, 80), "Save file"))
            {
                FilePanel.Settings settings = new FilePanel.Settings();
                settings.filters = new FilePanel.Filter[]
                {
                    new FilePanel.Filter("Text file (*.txt;*.log)", "txt", "log"),
                    new FilePanel.Filter("Image files (*.png;*.jpg;*.jpeg;*.tiff;*.gif;*.tga)", "png", "jpg", "jpeg", "tiff", "gif", "tga"),
                    new FilePanel.Filter("All files", "*"),
                };
                settings.title = "No save is actually performed";
                settings.initialFile = "Test.txt";

                message = "Canceled";
                FilePanel.SaveFilePanel(settings, (files) =>
                {
                    message = "Selected file\n" + string.Join("\n", files);
                });
            }

            GUI.TextArea(new Rect(x + 200, 10, 400, 400), message);
        }
    }
}