using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using UnityEngine;

namespace Kirurobo
{
    public class FileDialog
    {
        protected class LibUniWinC
        {
            [UnmanagedFunctionPointer((CallingConvention.Cdecl))]
            public delegate void StringCallback([MarshalAs(UnmanagedType.LPWStr)] string returnString);

            [DllImport("LibUniWinC")]
            public static extern bool RegisterOpenFilesCallback([MarshalAs(UnmanagedType.FunctionPtr)] StringCallback callback);

            [DllImport("LibUniWinC")]
            public static extern bool UnregisterOpenFilesCallback();

            [DllImport("LibUniWinC")]
            public static extern bool RegisterSaveFileCallback([MarshalAs(UnmanagedType.FunctionPtr)] StringCallback callback);

            [DllImport("LibUniWinC")]
            public static extern bool UnregisterSaveFileCallback();

            [DllImport("LibUniWinC")]
            public static extern void ShowOpenFilePanel(uint flags);

            [DllImport("LibUniWinC")]
            public static extern void ShowSaveFilePanel(uint flags);
        }

        /// <summary>
        /// ダイアログの設定フラグ
        /// </summary>
        [Flags]
        protected enum DialogFlag
        {
            None = 0,
            ChooseFiles = 1,
            ChooseDirectories = 2,
            AllowMultipleSelection = 4,
            CanCreateDirectories = 16,
        }

        /// <summary>
        /// ダイアログからファイル、フォルダが開かれた時に呼ばれるコールバック
        /// 文字列を配列に直すことと、フラグを立てるまで行う
        /// </summary>
        /// <param name="paths"></param>
        [MonoPInvokeCallback(typeof(LibUniWinC.StringCallback))]
        private static void _openFilesCallback([MarshalAs(UnmanagedType.LPWStr)] string paths)
        {
            lastOpenFiles = paths;
            wasOpened = true;
        }
        /// <summary>
        /// ダイアログからファイルが選択された時に呼ばれるコールバック
        /// 文字列を配列に直すことと、フラグを立てるまで行う
        /// </summary>
        /// <param name="paths"></param>
        [MonoPInvokeCallback(typeof(LibUniWinC.StringCallback))]
        private static void _saveFilesCallback([MarshalAs(UnmanagedType.LPWStr)] string paths)
        {
            lastSaveFiles = paths;
            wasSaved = true;
        }

        static string lastOpenFiles;
        static string lastSaveFiles;
        static bool wasSaved = false;
        static bool wasOpened = false;
        static bool shouldClose = false;

        public static void CloseAll()
        {
            shouldClose = true;
        }

        public static void OpenFilePanel()
        {
            UniWinCore.ShowOpenFilePanel();
        }

        public static void SaveFilePanel()
        {
            UniWinCore.ShowSaveFilePanel();
        }

        public static async Task OpenFilePanelAsync(Action<string[]> action)
        {
            LibUniWinC.RegisterOpenFilesCallback(_openFilesCallback);

            wasOpened = false;
            UniWinCore.ShowOpenFilePanel();

            while (!wasOpened && !shouldClose)
            {
                await Task.Delay(100);
            }
            LibUniWinC.UnregisterOpenFilesCallback();

            string[] files = UniWinCore.parsePaths(lastOpenFiles);
            action.Invoke(files);
        }

        public static async Task SaveFilePanelAsync(Action<string[]> action)
        {
            LibUniWinC.RegisterSaveFileCallback(_saveFilesCallback);

            wasSaved = false;
            UniWinCore.ShowSaveFilePanel();

            while (!wasSaved && !shouldClose)
            {
                await Task.Delay(100);
            }
            LibUniWinC.UnregisterSaveFileCallback();

            string[] files = UniWinCore.parsePaths(lastSaveFiles);
            action.Invoke(files);
        }
    }
}
