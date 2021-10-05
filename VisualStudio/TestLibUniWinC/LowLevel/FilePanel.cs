using AOT;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Kirurobo
{
    public class FilePanel
    {
        protected class LibUniWinC
        {
            [UnmanagedFunctionPointer((CallingConvention.Cdecl))]
            public delegate void StringCallback([MarshalAs(UnmanagedType.LPWStr)] string returnString);

            [DllImport("LibUniWinC")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool RegisterOpenFilesCallback([MarshalAs(UnmanagedType.FunctionPtr)] StringCallback callback);

            [DllImport("LibUniWinC", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool UnregisterOpenFilesCallback();

            [DllImport("LibUniWinC", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool RegisterSaveFileCallback([MarshalAs(UnmanagedType.FunctionPtr)] StringCallback callback);

            [DllImport("LibUniWinC")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool UnregisterSaveFileCallback();

            [DllImport("LibUniWinC")]
            public static extern void ShowOpenFilePanel(uint flags);

            [DllImport("LibUniWinC")]
            public static extern void ShowSaveFilePanel(uint flags);

            [DllImport("LibUniWinC", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.Bool)]
            //public static extern bool OpenFilePanel(ref PanelSettings settings, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder buffer, UInt32 bufferSize);
            public static extern bool OpenFilePanel(in PanelSettings settings, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder buffer, UInt32 bufferSize);

            [DllImport("LibUniWinC", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool OpenSavePanel(in PanelSettings settings, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder buffer, UInt32 bufferSize);

            [DllImport("LibUniWinC", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool OpenFolderPanel(in PanelSettings settings, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder buffer, UInt32 bufferSize);


            [StructLayout(LayoutKind.Sequential)]
            public struct PanelSettings {
                public Int32 structSize;
                public Int32 flags;
                public Int32 titleLength;
                [MarshalAs(UnmanagedType.LPWStr)] public readonly string lpTitleText;
                public Int32 filterLength;
                [MarshalAs(UnmanagedType.LPWStr)] public readonly string lpFilterText;
                public Int32 defaultPathLength;
                [MarshalAs(UnmanagedType.LPWStr)] public readonly string lpDefaultPath;

                public PanelSettings(Flag flags, string title, string filter, string path)
                {
                    this.structSize = Marshal.SizeOf<PanelSettings>();
                    this.flags = (Int32)flags;
                    this.titleLength = title.Length;
                    this.lpTitleText = title;
                    this.filterLength = filter.Length;
                    this.lpFilterText = filter;
                    this.defaultPathLength = path.Length;
                    this.lpDefaultPath = path;
                }

                public PanelSettings(Settings settings)
                {
                    this.structSize = Marshal.SizeOf<PanelSettings>();
                    this.flags = (Int32)settings.flags;

                    if (settings.title == null)
                    {
                        this.titleLength = 0;
                        this.lpTitleText = null;
                    } else
                    {
                        this.titleLength = settings.title.Length;
                        this.lpTitleText = settings.title;
                    }

                    if (settings.filter == null)
                    {
                        this.filterLength = 0;
                        this.lpFilterText = null;
                    }
                    else
                    {
                        this.filterLength = settings.filter.Length;
                        this.lpFilterText = settings.filter;
                    }

                    if (settings.path == null)
                    {
                        this.defaultPathLength = 0;
                        this.lpDefaultPath = null;
                    }
                    else
                    {
                        this.defaultPathLength = settings.path.Length;
                        this.lpDefaultPath = settings.path;
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
            CanCreateDirectories = 16,
        }

        /// <summary>
        /// Parameters for file dialog
        /// </summary>
        public struct Settings
        {
            public string title;
            public string filter;
            public string path;
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

            if (LibUniWinC.OpenFilePanel(in ps, sb, (uint)sb.Capacity))
            {
                string[] files = UniWinCore.parsePaths(sb.ToString());
                action.Invoke(files);
            }
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
        }
    }
}
