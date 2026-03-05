# WhiskrIO Windows

Windows 向けの最小実装です。トレイ常駐し、グローバルホットキーで録音→Gemini文字起こし→貼り付けを行います。

## 必要要件
- Windows 10/11
- .NET SDK 8

## 実行
```powershell
cd WhiskrIO.Windows
dotnet restore
dotnet run
```

## 設定
`%AppData%/WhiskrIO/settings.json` を編集してください。

- `GeminiApiKey`: 初回は平文で入れて起動すると、次回保存時にDPAPIで暗号化されます。
- `GeminiModel`: 例 `gemini-2.5-flash-lite`
- `HotkeyModifiers`: Alt=1, Ctrl=2, Shift=4, Win=8
- `HotkeyVirtualKey`: 仮想キーコード（例 `82` = R）
