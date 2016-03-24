using System;
using System.Windows;
using System.Runtime.InteropServices;

namespace Client
{
    // CLASS TAKEN FROM STACK
    public class FlashWindow
    {
        private IntPtr mainWindowHWnd;
        private Application theApp;

        public FlashWindow(Application app)
        {
            theApp = app;
        }

        public void FlashApplicationWindow()
        {
            InitializeHandle();
            Flash(mainWindowHWnd, 5);
        }

        public void StopFlashing()
        {
            InitializeHandle();

            if (Win2000OrLater)
            {
                FLASHWINFO fi = CreateFlashInfoStruct(mainWindowHWnd, FLASHW_STOP, uint.MaxValue, 0);
                FlashWindowEx(ref fi);
            }
        }

        private void InitializeHandle()
        {
            if (mainWindowHWnd == IntPtr.Zero)
            {
                var mainWindow = theApp.MainWindow;
                mainWindowHWnd = new System.Windows.Interop.WindowInteropHelper(mainWindow).Handle;
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        public const uint FLASHW_STOP = 0;
        public const uint FLASHW_CAPTION = 1;
        public const uint FLASHW_TRAY = 2;
        public const uint FLASHW_ALL = 3;
        public const uint FLASHW_TIMER = 4;
        public const uint FLASHW_TIMERNOFG = 12;

        private static FLASHWINFO CreateFlashInfoStruct(IntPtr handle, uint flags, uint count, uint timeout)
        {
            FLASHWINFO fi = new FLASHWINFO();
            fi.cbSize = Convert.ToUInt32(Marshal.SizeOf(fi));
            fi.hwnd = handle;
            fi.dwFlags = flags;
            fi.uCount = count;
            fi.dwTimeout = timeout;
            return fi;
        }

        public static bool Flash(IntPtr hwnd)
        {
            if (Win2000OrLater)
            {
                FLASHWINFO fi = CreateFlashInfoStruct(hwnd, FLASHW_ALL | FLASHW_TIMERNOFG, uint.MaxValue, 0);
                return FlashWindowEx(ref fi);
            }
            return false;
        }

        public static bool Flash(IntPtr hwnd, uint count)
        {
            if (Win2000OrLater)
            {
                FLASHWINFO fi = CreateFlashInfoStruct(hwnd, FLASHW_ALL | FLASHW_TIMERNOFG, count, 0);
                return FlashWindowEx(ref fi);
            }
            return false;
        }

        private static bool Win2000OrLater
        {
            get { return Environment.OSVersion.Version.Major >= 5; }
        }
    }
}
