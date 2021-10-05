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
            public struct PanelSettings : IDisposable {
                public Int32 structSize;
                public Int32 flags;
                public IntPtr lpTitleText;
                public IntPtr lpFilterText;
                public IntPtr lpDefaultPath;

                public PanelSettings(Settings settings)
                {
                    //this.structSize = 4 * 2 + Marshal.SizeOf<IntPtr>() * 3;
                    this.structSize = Marshal.SizeOf<PanelSettings>();
                    this.flags = (Int32)settings.flags;

                    //this.lpTitleText = IntPtr.Zero;
                    //this.lpFilterText = IntPtr.Zero;
                    //this.lpDefaultPath = IntPtr.Zero;
                    this.lpTitleText = Marshal.StringToHGlobalUni(settings.title);
                    this.lpFilterText = Marshal.StringToHGlobalUni(settings.filter);
                    this.lpDefaultPath = Marshal.StringToHGlobalUni(settings.path);

                    //this.structSize = Marshal.SizeOf(this);
                }

                public void Dispose()
                {
                    if (this.lpTitleText != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(lpTitleText);
                        this.lpTitleText = IntPtr.Zero;
                    }

                    if (this.lpFilterText!= IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(lpFilterText);
                        this.lpFilterText= IntPtr.Zero;
                    }

                    if (this.lpDefaultPath!= IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(lpDefaultPath);
                        this.lpDefaultPath= IntPtr.Zero;
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
