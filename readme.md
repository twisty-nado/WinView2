# WinView2

A lightweight WPF shell that wraps WebView2, letting you build Windows desktop apps with plain HTML, CSS, and JavaScript - while still having access to native window controls, Mica/Acrylic backdrops, and dark mode through a simple JS API.

## Features

- Native Mica, Mica Alt, and Acrylic backdrop support (Windows 11)
- Dark mode titlebar control
- Full window management from JS (move, resize, minimize, maximize)
- Automatic favicon sync to the titlebar
- Event-driven window state pushed to JS

## Important notices

- This does not host the website with Node.JS or any other frameworks, this just uses `file:///` to preview the page
- I have no idea how I wrote any of this, so I recommend cloning this repo to adjust the code on your own, since I will not be making any releases in the future unless a miracle happens.

## Requirements

- Windows 10/11
- [WebView2 Runtime](https://developer.microsoft.com/en-us/microsoft-edge/webview2/)
- .NET Framework 4.8

## Getting Started

Put your web content in the `source/` folder. `source/index.html` is the entry point.

```
WinView2/
└── source/
    └── index.html   ← your app goes here
```

Access the window API in your JS via:

```js
const api = window.chrome.webview.hostObjects.windowApi;
```

> All API calls return promises - always `await` them.

---

## API Reference

### Window Control

#### `Close()`
Closes the application window.
```js
await api.Close();
```

#### `Minimize()`
Minimizes the window to the taskbar.
```js
await api.Minimize();
```

#### `Maximize()`
Maximizes the window to fill the screen.
```js
await api.Maximize();
```

#### `Restore()`
Restores the window to its previous size and position.
```js
await api.Restore();
```

#### `SetSize(width, height)`
Resizes the window to the given pixel dimensions (outer window, not content area).
```js
await api.SetSize(1280, 720);
```

#### `Move(x, y)`
Moves the window so its top-left corner is at `(x, y)` in screen coordinates.
```js
await api.Move(100, 100);
```

---

### Appearance

#### `SetBackdrop(type)`
Sets the window backdrop effect. Only works on Windows 11.

| Value | Effect |
|---|---|
| `"mica"` | Blurs and tints the desktop wallpaper (default) |
| `"acrylic"` | Blurs content behind the window |
| `"tabbed"` | Stronger Mica variant, used in File Explorer |
| `"none"` | No effect, solid background |

```js
await api.SetBackdrop("mica");
```

#### `SetDarkMode(dark)`
Switches the native window titlebar between light and dark. Does not affect WebView content - handle that yourself via CSS.

```js
await api.SetDarkMode(true);
```

> Tip: sync your UI theme alongside it:
> ```js
> async function setTheme(dark) {
>     await api.SetDarkMode(dark);
>     document.documentElement.setAttribute("data-theme", dark ? "dark" : "light");
> }
> ```

---

### State & Info

#### `GetWindowInfo()`
Returns a JSON string with the full current window state.

```js
const info = JSON.parse(await api.GetWindowInfo());
// {
//   x: 100,
//   y: 100,
//   width: 1280,
//   height: 720,
//   isMaximized: false,
//   isMinimized: false,
//   isFocused: true,
//   isDarkMode: false
// }
```

#### `IsMaximized()`
Returns `true` if the window is maximized.

#### `IsMinimized()`
Returns `true` if the window is minimized.

#### `IsFocused()`
Returns `true` if the window has focus.

#### `IsDarkMode()`
Returns `true` if dark mode is active.

#### `IsWindows11()`
Returns `true` if running on Windows 11 (build 22000+). Use this to gate backdrop features.
```js
if (await api.IsWindows11()) {
    await api.SetBackdrop("mica");
}
```

---

### Events

WinView2 automatically pushes window state changes to JS. The event `detail` has the same shape as `GetWindowInfo()`.

#### `windowStateChanged`
Fired when the window is minimized, maximized, or restored.
```js
window.addEventListener("windowStateChanged", (e) => {
    console.log(e.detail.isMaximized);
});
```

#### `windowMoved`
Fired when the window is moved.
```js
window.addEventListener("windowMoved", (e) => {
    console.log(e.detail.x, e.detail.y);
});
```

---

## Notes

- Backdrop and dark mode require **Windows 11** - always check `IsWindows11()` before calling them
- `SetDarkMode` only affects the native chrome (titlebar/border), not your web content
- `SetSize` and `Move` operate on the **outer window**, not the WebView content area
- The taskbar icon uses your app's `.ico` resource - the favicon only syncs to the titlebar
