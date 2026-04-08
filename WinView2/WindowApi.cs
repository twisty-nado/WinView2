using System.Text.Json;
using System.Windows;

namespace WinView2
{
    [System.Runtime.InteropServices.ComVisible(true)]
    public class WindowApi
    {
        private readonly MainWindow _window;
        private bool _isDark = false;

        public WindowApi(MainWindow window) => _window = window;

        // actions
        public void Close() => _window.Dispatcher.Invoke(() => _window.Close());
        public void Minimize() => _window.Dispatcher.Invoke(() => _window.WindowState = WindowState.Minimized);
        public void Maximize() => _window.Dispatcher.Invoke(() => _window.WindowState = WindowState.Maximized);
        public void Restore() => _window.Dispatcher.Invoke(() => _window.WindowState = WindowState.Normal);
        public void SetSize(int width, int height) => _window.Dispatcher.Invoke(() =>
        {
            _window.Width = width;
            _window.Height = height;
        });
        public void Move(int x, int y) => _window.Dispatcher.Invoke(() =>
        {
            _window.Left = x;
            _window.Top = y;
        });

        public void SetDarkMode(bool dark) => _window.Dispatcher.Invoke(() =>
        {
            _isDark = dark;
            MicaHelper.SetDarkMode(_window, dark);
        });

        public void SetBackdrop(string type) => _window.Dispatcher.Invoke(() =>
            MicaHelper.ApplyBackdrop(_window, type));

        // getters
        public bool IsMaximized() => _window.Dispatcher.Invoke(() => _window.WindowState == WindowState.Maximized);
        public bool IsMinimized() => _window.Dispatcher.Invoke(() => _window.WindowState == WindowState.Minimized);
        public bool IsFocused() => _window.Dispatcher.Invoke(() => _window.IsActive);
        public bool IsDarkMode() => _isDark;
        public bool IsWindows11() => MicaHelper.IsWindows11;
        public string GetWindowInfo() => GetWindowInfo(_window, _isDark);

        public static string GetWindowInfo(MainWindow w, bool dark = false) => w.Dispatcher.Invoke(() =>
            JsonSerializer.Serialize(new
            {
                x = w.Left,
                y = w.Top,
                width = w.Width,
                height = w.Height,
                isMaximized = w.WindowState == WindowState.Maximized,
                isMinimized = w.WindowState == WindowState.Minimized,
                isFocused = w.IsActive,
                isDarkMode = dark
            })
        );
    }
}