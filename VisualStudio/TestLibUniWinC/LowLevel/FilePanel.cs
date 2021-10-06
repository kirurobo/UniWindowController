﻿using AOT;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Kirurobo
{
    public class FilePanel
    {
        protected class LibUniWinC
        {
            [DllImport("LibUniWinC", CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool OpenFilePanel(in PanelSettings settings, [MarshalAs(UnmanagedType.LPWStr), Out] StringBuilder buffer, UInt32 bufferSize);

            //[DllImport("LibUniWinC", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            [DllImport("LibUniWinC", CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            //public static extern bool OpenFilePanelTest();
            public static extern bool OpenFilePanelTest([MarshalAs(UnmanagedType.LPWStr), Out] StringBuilder buffer, UInt32 bufferSize);
            //public static extern bool OpenFilePanelTest([In] PanelSettings settings);

            [DllImport("LibUniWinC", CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool OpenSavePanel(in PanelSettings settings, [MarshalAs(UnmanagedType.LPWStr), Out] StringBuilder buffer, UInt32 bufferSize);


            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct PanelSettings : IDisposable {
                public Int32 structSize;
                public Int32 flags;
                public IntPtr lpszTitle;
                public IntPtr lpszFilter;
                public IntPtr lpszInitialFile;
                public IntPtr lpszInitialDir;
                public IntPtr lpszDefaultExt;

                public PanelSettings(Settings settings)
                {
                    this.structSize = 0;
                    //this.structSize = 4 * 2 + Marshal.SizeOf<IntPtr>() * 3;
                    this.flags = (Int32)settings.flags;

                    //this.lpTitleText = IntPtr.Zero;
                    //this.lpFilterText = IntPtr.Zero;
                    //this.lpDefaultPath = IntPtr.Zero;
                    this.lpszTitle = Marshal.StringToHGlobalUni(settings.title);
                    this.lpszFilter = Marshal.StringToHGlobalUni(settings.filter);
                    this.lpszInitialFile = Marshal.StringToHGlobalUni(settings.initialFile);
                    this.lpszInitialDir = Marshal.StringToHGlobalUni(settings.initialDirectory);
                    this.lpszDefaultExt = Marshal.StringToHGlobalUni(settings.defaultExtension);

                    //this.structSize = Marshal.SizeOf(this);
                    this.structSize = Marshal.SizeOf(this);
                }

                public void Dispose()
                {
                    if (this.lpszTitle != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(lpszTitle);
                        this.lpszTitle = IntPtr.Zero;
                    }

                    if (this.lpszFilter!= IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(lpszFilter);
                        this.lpszFilter= IntPtr.Zero;
                    }

                    if (this.lpszInitialFile!= IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(lpszInitialFile);
                        this.lpszInitialFile= IntPtr.Zero;
                    }

                    if (this.lpszInitialDir != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(lpszInitialDir);
                        this.lpszInitialDir = IntPtr.Zero;
                    }

                    if (this.lpszDefaultExt != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(lpszDefaultExt);
                        this.lpszDefaultExt = IntPtr.Zero;
                    }
                }
            }

        }

        /// <summary>
        /// ダイアログの設定フラグ
        /// </summary>
        [Flags]
        public enum Flag
        {
            None = 0,
            ChooseFiles = 1,
            ChooseDirectories = 2,
            AllowMultipleSelection = 4,
            RetrieveLinkTarget = 8,
            CanCreateDirectories = 16,
        }

        /// <summary>
        /// Parameters for file dialog
        /// </summary>
        public struct Settings
        {
            public string title;
            public string filter;
            public string initialDirectory;
            public string initialFile;
            public string defaultExtension;
            public Flag flags;
        }

        /// <summary>
        /// ファイルやフォルダ―のパス受け渡しUTF-16バッファの文字数
        ///     複数パスが改行区切りで入るため 260 では少ない。
        /// </summary>
        private const int pathBufferSize = 2560;


        /// <summary>
        /// Open file selection dialog
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="action"></param>
        public static void OpenFilePanel(Settings settings, Action<string[]> action)
        {
            LibUniWinC.PanelSettings ps = new LibUniWinC.PanelSettings(settings);

            StringBuilder sb = new StringBuilder(pathBufferSize);

            // LibUniWinC.OpenFilePanelTest(ps);
            //if (false)
            //if (LibUniWinC.OpenFilePanelTest(0))
            //if (LibUniWinC.OpenFilePanelTest(sb, (uint)sb.Capacity))
            if (LibUniWinC.OpenFilePanel(in ps, sb, (uint)sb.Capacity))
            {
                string[] files = UniWinCore.parsePaths(sb.ToString());
                action.Invoke(files);
            }
            Console.WriteLine("C# PanelSettings: " + ps.structSize);
            Console.WriteLine(sb);

            ps.Dispose();   // Settings を渡したコンストラクタでメモリが確保されるため、解放が必要
        }

        /// <summary>
        /// Open save-file selection dialog
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="action"></param>
        public static void SaveFilePanel(Settings settings, Action<string[]> action)
        {
            LibUniWinC.PanelSettings ps = new LibUniWinC.PanelSettings(settings);

            StringBuilder sb = new StringBuilder(pathBufferSize);

            if (LibUniWinC.OpenSavePanel(in ps, sb, (uint)sb.Capacity))
            {
                string[] files = UniWinCore.parsePaths(sb.ToString());
                action.Invoke(files);
            }

            ps.Dispose();   // Settings を渡したコンストラクタでメモリが確保されるため、解放が必要
        }
    }
}