using Microsoft.Web.WebView2.Core;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace WinView2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            ContentRendered += MainWindow_Shown;
        }

        private async void MainWindow_Shown(object sender, EventArgs e)
        {
            if (MicaHelper.IsWindows11)
            {
                MicaHelper.ApplyBackdrop(this, "mica");
                Background = null;
            }
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await webView.EnsureCoreWebView2Async();

            webView.CoreWebView2.AddHostObjectToScript("windowApi", new WindowApi(this));

            webView.CoreWebView2.DocumentTitleChanged += (s, _) =>
                Title = webView.CoreWebView2.DocumentTitle;

            webView.CoreWebView2.NavigationCompleted += OnNavigationCompleted;

            StateChanged += async (s, _) =>
                await DispatchWindowEventAsync("windowStateChanged");

            LocationChanged += async (s, _) =>
                await DispatchWindowEventAsync("windowMoved");

            string sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "source", "index.html");
            webView.Source = new Uri(sourcePath);
        }

        private async void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            try
            {
                var faviconUri = await GetBestFaviconUriAsync();

                if (faviconUri != null)
                {
                    var bitmap = TitlebarIconHelper.LoadBitmapFromUri(faviconUri);
                    if (bitmap != null)
                    {
                        TitlebarIconHelper.SetTitlebarIcon(this, bitmap);
                        return;
                    }
                }

                // fallback: use WebView2's built-in favicon
                var stream = await webView.CoreWebView2.GetFaviconAsync(CoreWebView2FaviconImageFormat.Png);
                if (stream != null)
                {
                    var bitmap = await TitlebarIconHelper.LoadBitmapFromStreamAsync(stream);
                    if (bitmap != null)
                        TitlebarIconHelper.SetTitlebarIcon(this, bitmap);
                }
            }
            catch { }
        }

        private async Task<Uri?> GetBestFaviconUriAsync()
        {
            const string script = """
                (() => {
                    const links = Array.from(document.querySelectorAll('link[rel~="icon"]'));
                    return links.map(l => ({ href: l.href, sizes: l.sizes?.value ?? '' }));
                })();
                """;

            try
            {
                var result = await webView.CoreWebView2.ExecuteScriptAsync(script);
                var icons = JsonSerializer.Deserialize<List<FaviconInfo>>(result);
                if (icons == null || icons.Count == 0) return null;

                var best = icons
                    .Select(i => (icon: i, size: ParseSize(i.sizes)))
                    .OrderByDescending(x => x.size)
                    .FirstOrDefault()
                    .icon;

                return best != null ? new Uri(best.href) : null;
            }
            catch
            {
                return null;
            }
        }

        private static int ParseSize(string sizes)
        {
            if (string.IsNullOrEmpty(sizes)) return 0;
            var parts = sizes.Split('x');
            return parts.Length == 2 && int.TryParse(parts[0], out var n) ? n : 0;
        }

        private async Task DispatchWindowEventAsync(string eventName)
        {
            await webView.CoreWebView2.ExecuteScriptAsync(
                $"window.dispatchEvent(new CustomEvent('{eventName}', {{ detail: {WindowApi.GetWindowInfo(this)} }}))"
            );
        }

        private class FaviconInfo
        {
            public string? href { get; set; }
            public string? sizes { get; set; }
        }
    }
}