using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 160, 80), "Open a file"))
        {
            Kirurobo.FilePanel.Settings settings = new Kirurobo.FilePanel.Settings();
            settings.filters = new Kirurobo.FilePanel.Filter[]
            {
                new Kirurobo.FilePanel.Filter("Image files", "png", "jpg", "jpeg", "tiff", "gif", "tga"),
                new Kirurobo.FilePanel.Filter("Documents", "txt", "rtf", "doc", "docx"),
                new Kirurobo.FilePanel.Filter("All files", "*")
            };
            settings.title = "Open a file!";
            settings.initialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);

            message = "Canceled";
            Kirurobo.FilePanel.OpenFilePanel(settings, (files) => {
                message = "Open a file\n" + string.Join("\n", files);
            });
        }

        if (GUI.Button(new Rect(10, 100, 160, 80), "Open multiple files"))
        {
            Kirurobo.FilePanel.Settings settings = new Kirurobo.FilePanel.Settings();
            settings.filters = new Kirurobo.FilePanel.Filter[]
            {
                new Kirurobo.FilePanel.Filter("Image files", "png", "jpg", "jpeg", "tiff", "gif", "tga"),
                new Kirurobo.FilePanel.Filter("Documents", "txt", "rtf", "doc", "docx"),
                new Kirurobo.FilePanel.Filter("All files", "*")
            };
            settings.flags = Kirurobo.FilePanel.Flag.AllowMultipleSelection;
            settings.title = "Open multiple files!";
            settings.initialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);

            message = "Canceled";
            Kirurobo.FilePanel.OpenFilePanel(settings, (files) => {
                message = "Open multiple files\n" + string.Join("\n", files);
            });
        }

        if (GUI.Button(new Rect(10, 200, 160, 80), "Save file"))
        {
            Kirurobo.FilePanel.Settings settings = new Kirurobo.FilePanel.Settings();
            settings.filters = new Kirurobo.FilePanel.Filter[]
            {
                new Kirurobo.FilePanel.Filter("Text file", "txt", "log"),
                new Kirurobo.FilePanel.Filter("Image files", "png", "jpg", "jpeg", "tiff", "gif", "tga"),
                new Kirurobo.FilePanel.Filter("All files", "*")
            };
            settings.title = "No save is actually performed";
            settings.initialFile = "Test.txt";

            message = "Canceled";
            Kirurobo.FilePanel.SaveFilePanel(settings, (files) => {
                message = "Selected file\n" + string.Join("\n", files);
            });
        }

        GUI.TextArea(new Rect(200, 10, 400, 400), message);
    }
}
