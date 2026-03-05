using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace WhiskrIO.Windows.Services;

public sealed class GeminiService
{
    private readonly AppSettingsStore _settingsStore;
    private readonly HttpClient _httpClient = new();

    public GeminiService(AppSettingsStore settingsStore)
    {
        _settingsStore = settingsStore;
    }

    public async Task<string> TranscribeAsync(string audioPath)
    {
        var settings = _settingsStore.Load();
        if (string.IsNullOrWhiteSpace(settings.GeminiApiKey))
        {
            return "[WhiskrIO] settings.json に Gemini API キーを設定してください。";
        }

        var bytes = await File.ReadAllBytesAsync(audioPath);
        var payload = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new object[]
                    {
                        new { text = "音声を自然な日本語に文字起こししてください。" },
                        new
                        {
                            inlineData = new
                            {
                                mimeType = "audio/wav",
                                data = Convert.ToBase64String(bytes)
                            }
                        }
                    }
                }
            }
        };

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{settings.GeminiModel}:generateContent?key={settings.GeminiApiKey}";
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await _httpClient.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            return $"[WhiskrIO] Gemini API error: {response.StatusCode}";
        }

        using var document = JsonDocument.Parse(body);
        var text = document.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return text ?? string.Empty;
    }
}
