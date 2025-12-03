using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Kirurobo
{
    /// <summary>
    /// Basic filepanel sample
    /// </summary>
    public class FileDialogSample : MonoBehaviour
    {
        public Button openFileButton;
        public Button openMultipleFilesButton;
        public Button saveFileButton;
        public Text messageText;

        // Start is called before the first frame update
        void Start()
        {
            openFileButton.onClick.AddListener(OpenSingleFile);
            openMultipleFilesButton.onClick.AddListener(OpenMultipleFiles);
            saveFileButton.onClick.AddListener(OpenSaveFile);
            messageText.text = "Click a button!";
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// Open the open file dialog to select single file.
        /// </summary>
        private void OpenSingleFile() {
            FilePanel.Settings settings = new FilePanel.Settings();
            settings.filters = new FilePanel.Filter[]
            {
                new FilePanel.Filter("All files", "*"),
                new FilePanel.Filter("Image files (*.png;*.jpg;*.jpeg;*.tiff;*.gif;*.tga)", "png", "jpg", "jpeg", "tiff", "gif", "tga"),
                new FilePanel.Filter("Documents (*.txt;*.rtf;*.doc;*.docx)", "txt", "rtf", "doc", "docx"),
            };
            settings.title = "Open a file!";
            settings.initialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);

            messageText.text = "";
            FilePanel.OpenFilePanel(settings, (files) =>
            {
                messageText.text = "Open a file\n" + string.Join("\n", files);
            });
        }

        /// <summary>
        /// Open the open file dialog to select multiple files.
        /// </summary>
        private void OpenMultipleFiles() {
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

            messageText.text = "";
            FilePanel.OpenFilePanel(settings, (files) =>
            {
                messageText.text = "Open multiple files\n" + string.Join("\n", files);
            });
        }

        /// <summary>
        /// Open the save file dialog.
        /// </summary>
        private void OpenSaveFile() {
            FilePanel.Settings settings = new FilePanel.Settings();
            settings.filters = new FilePanel.Filter[]
            {
                new FilePanel.Filter("Text file (*.txt;*.log)", "txt", "log"),
                new FilePanel.Filter("Image files (*.png;*.jpg;*.jpeg;*.tiff;*.gif;*.tga)", "png", "jpg", "jpeg", "tiff", "gif", "tga"),
                new FilePanel.Filter("All files", "*"),
            };
            settings.title = "No save is actually performed";
            settings.initialFile = "Test.txt";

            messageText.text = "";
            FilePanel.SaveFilePanel(settings, (files) =>
            {
                messageText.text = "Selected file\n" + string.Join("\n", files);
            });
        }
    }
}