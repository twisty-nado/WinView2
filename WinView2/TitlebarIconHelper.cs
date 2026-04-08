using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace WinView2
{
    internal static class TitlebarIconHelper
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

        private const int WM_SETICON = 0x0080;
        private const int ICON_SMALL = 0; // titlebar only, skips taskbar

        public static void SetTitlebarIcon(Window window, BitmapSource bitmap)
        {
            var hwnd = new WindowInteropHelper(window).Handle;

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using var ms = new MemoryStream();
            encoder.Save(ms);
            ms.Position = 0;

            using var gdiBitmap = new Bitmap(ms);
            var hIcon = gdiBitmap.GetHicon();

            try
            {
                SendMessage(hwnd, WM_SETICON, ICON_SMALL, hIcon);
            }
            finally
            {
                DestroyIcon(hIcon);
            }
        }

        /// <summary>
        /// Loads a BitmapSource from a URI (PNG or ICO).
        /// Returns null if it fails.
        /// </summary>
        public static BitmapSource? LoadBitmapFromUri(Uri uri)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = uri;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Loads a BitmapSource from a stream (e.g. WebView2 favicon stream).
        /// Returns null if it fails.
        /// </summary>
        public static async Task<BitmapSource?> LoadBitmapFromStreamAsync(Stream stream)
        {
            try
            {
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                ms.Position = 0;

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }
    }
}