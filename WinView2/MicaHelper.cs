using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace WinView2
{
    public static class MicaHelper
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

        [StructLayout(LayoutKind.Sequential)]
        private struct MARGINS { public int Left, Right, Top, Bottom; }

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
        private const int DWMSBT_NONE = 1;
        private const int DWMSBT_MAINWINDOW = 2;
        private const int DWMSBT_TRANSIENTWINDOW = 3;
        private const int DWMSBT_TABBEDWINDOW = 4;

        public static bool IsWindows11 => Environment.OSVersion.Version.Build >= 22000;

        public static void SetDarkMode(Window window, bool dark)
        {
            if (!IsWindows11) return;
            var hwnd = new WindowInteropHelper(window).Handle;
            int value = dark ? 1 : 0;
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref value, sizeof(int));
        }

        public static void ApplyBackdrop(Window window, string type = "mica")
        {
            if (!IsWindows11) return;

            var hwnd = new WindowInteropHelper(window).Handle;
            int value = type switch
            {
                "mica" => DWMSBT_MAINWINDOW,
                "acrylic" => DWMSBT_TRANSIENTWINDOW,
                "tabbed" => DWMSBT_TABBEDWINDOW,
                "none" => DWMSBT_NONE,
                _ => DWMSBT_MAINWINDOW
            };

            var hwndSource = HwndSource.FromHwnd(hwnd);
            if (hwndSource != null)
            {
                hwndSource.CompositionTarget.BackgroundColor = value != DWMSBT_NONE
                    ? System.Windows.Media.Color.FromArgb(0, 0, 0, 0)
                    : System.Windows.Media.Color.FromArgb(255, 255, 255, 255);
            }

            DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref value, sizeof(int));

            var margins = value != DWMSBT_NONE
                ? new MARGINS { Left = -1, Right = -1, Top = -1, Bottom = -1 }
                : new MARGINS { Left = 0, Right = 0, Top = 0, Bottom = 0 };
            DwmExtendFrameIntoClientArea(hwnd, ref margins);
        }
    }
}