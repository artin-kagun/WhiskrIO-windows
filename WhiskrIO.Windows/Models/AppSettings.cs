namespace WhiskrIO.Windows.Models;

public sealed class AppSettings
{
    public string GeminiApiKey { get; set; } = string.Empty;
    public string GeminiModel { get; set; } = "gemini-2.5-flash-lite";
    public uint HotkeyModifiers { get; set; } = GlobalHotkeyModifiers.MOD_CONTROL | GlobalHotkeyModifiers.MOD_SHIFT;
    public uint HotkeyVirtualKey { get; set; } = 0x52; // R
}

public static class GlobalHotkeyModifiers
{
    public const uint MOD_ALT = 0x0001;
    public const uint MOD_CONTROL = 0x0002;
    public const uint MOD_SHIFT = 0x0004;
    public const uint MOD_WIN = 0x0008;
}
