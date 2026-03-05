using WhiskrIO.Windows.Models;
using WhiskrIO.Windows.Utils;

namespace WhiskrIO.Windows.Services;

public sealed class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly AppSettingsStore _settingsStore;
    private readonly GeminiService _geminiService;
    private readonly AudioRecorderService _audioRecorder;
    private readonly GlobalHotkeyManager _hotkeyManager;

    public TrayApplicationContext()
    {
        _settingsStore = new AppSettingsStore();
        _geminiService = new GeminiService(_settingsStore);
        _audioRecorder = new AudioRecorderService();
        _hotkeyManager = new GlobalHotkeyManager();

        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Information,
            Text = "WhiskrIO Windows",
            Visible = true,
            ContextMenuStrip = BuildMenu()
        };

        _hotkeyManager.HotkeyPressed += async (_, _) => await HandleToggleRecordingAsync();

        var settings = _settingsStore.Load();
        _hotkeyManager.Register(settings.HotkeyModifiers, settings.HotkeyVirtualKey);
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("録音開始/停止", null, async (_, _) => await HandleToggleRecordingAsync());
        menu.Items.Add("設定ファイルを開く", null, (_, _) => OpenSettingsFile());
        menu.Items.Add("終了", null, (_, _) => ExitThread());
        return menu;
    }

    private void OpenSettingsFile()
    {
        var settingsPath = _settingsStore.EnsureFileExists();
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = "notepad.exe",
            Arguments = settingsPath,
            UseShellExecute = false
        });
    }

    private async Task HandleToggleRecordingAsync()
    {
        if (!_audioRecorder.IsRecording)
        {
            _audioRecorder.StartRecording();
            _notifyIcon.ShowBalloonTip(1000, "WhiskrIO", "録音開始", ToolTipIcon.Info);
            return;
        }

        var audioPath = await _audioRecorder.StopRecordingAsync();
        _notifyIcon.ShowBalloonTip(1000, "WhiskrIO", "文字起こし中...", ToolTipIcon.Info);

        var text = await _geminiService.TranscribeAsync(audioPath);
        TextInjector.InjectText(text);
        _notifyIcon.ShowBalloonTip(1000, "WhiskrIO", "入力完了", ToolTipIcon.Info);
    }

    protected override void ExitThreadCore()
    {
        _hotkeyManager.Dispose();
        _audioRecorder.Dispose();
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        base.ExitThreadCore();
    }
}
