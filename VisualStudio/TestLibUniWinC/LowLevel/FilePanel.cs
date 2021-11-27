using AOT;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Kirurobo
{
    /// <summary>
    /// Provides static methods to open native file dialog
    /// </summary>
    public class FilePanel
    {
        protected class LibUniWinC
        {
            [DllImport("LibUniWinC", CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool OpenFilePanel(in PanelSettings settings, [MarshalAs(UnmanagedType.LPWStr), Out] StringBuilder buffer, UInt32 bufferSize);

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
                    this.lpszFilter = Marshal.StringToHGlobalUni(Filter.Join(settings.filters));
                    this.lpszInitialFile = Marshal.StringToHGlobalUni(settings.initialFile);
                    this.lpszInitialDir = Marshal.StringToHGlobalUni(settings.initialDirectory);
                    //this.lpszDefaultExt = Marshal.StringToHGlobalUni(settings.defaultExtension);
                    this.lpszDefaultExt = IntPtr.Zero;

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
            FileMustExist = 1,            // Windows only
            FolderMustExist = 2,          // Windows only
            AllowMultipleSelection = 4,
            CanCreateDirectories = 16,
            OverwritePrompt = 256,        // Always enabled on macOS
            CreatePrompt = 512,           // Always enabled on macOS
            ShowHiddenFiles = 4096,
            RetrieveLink = 8192,
        }

        /// <summary>
        /// Parameters for file dialog
        /// </summary>
        public struct Settings
        {
            public string title;
            public Filter[] filters;
            public string initialDirectory;
            public string initialFile;
            public string defaultExtension;    // Not implemented
            public Flag flags;
        }

        /// <summary>
        /// File filter
        /// </summary>
        public class Filter
        {
            protected string title;
            protected string[] extensions;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="title">Filter title. (Not available on macOS yet)</param>
            /// <param name="extensions">Extensions like ["png", "jpg", "txt"]</param>
            public Filter(string title, params string[] extensions)
            {
                this.title = title;
                this.extensions = extensions;
            }

            public override string ToString()
            {
                return title + "\t" + String.Join("\t", extensions);
            }

            /// <summary>
            /// Returns converted string from Filter array
            /// </summary>
            /// <param name="filters"></param>
            /// <returns></returns>
            public static string Join(Filter[] filters)
            {
                if (filters == null) return "";

                string result = "";
                bool isFirstItem = true;
                foreach (var filter in filters) {
                    if (!isFirstItem) result += "\n";
                    result += filter.ToString();
                    isFirstItem = false;
                }
                return result;
            }
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

            if (LibUniWinC.OpenFilePanel(in ps, sb, (uint)sb.Capacity))
            {
                string[] files = UniWinCore.parsePaths(sb.ToString());
                action.Invoke(files);
            }

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
