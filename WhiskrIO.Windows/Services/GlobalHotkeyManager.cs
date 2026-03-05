using System.Runtime.InteropServices;

namespace WhiskrIO.Windows.Services;

public sealed class GlobalHotkeyManager : IDisposable
{
    private const int WmHotkey = 0x0312;
    private readonly HotkeyWindow _window = new();
    private int _hotkeyId = 1;

    public event EventHandler? HotkeyPressed;

    public GlobalHotkeyManager()
    {
        _window.HotkeyPressed += (_, _) => HotkeyPressed?.Invoke(this, EventArgs.Empty);
    }

    public void Register(uint modifiers, uint virtualKey)
    {
        UnregisterHotKey(_window.Handle, _hotkeyId);
        RegisterHotKey(_window.Handle, _hotkeyId, modifiers, virtualKey);
    }

    public void Dispose()
    {
        UnregisterHotKey(_window.Handle, _hotkeyId);
        _window.DestroyHandle();
    }

    private sealed class HotkeyWindow : NativeWindow
    {
        public event EventHandler? HotkeyPressed;

        public HotkeyWindow()
        {
            CreateHandle(new CreateParams());
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmHotkey)
            {
                HotkeyPressed?.Invoke(this, EventArgs.Empty);
            }

            base.WndProc(ref m);
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
