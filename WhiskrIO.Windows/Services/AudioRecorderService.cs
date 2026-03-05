using NAudio.Wave;

namespace WhiskrIO.Windows.Services;

public sealed class AudioRecorderService : IDisposable
{
    private WaveInEvent? _waveIn;
    private WaveFileWriter? _writer;
    private string? _currentFile;

    public bool IsRecording => _waveIn is not null;

    public void StartRecording()
    {
        if (_waveIn is not null)
        {
            return;
        }

        _currentFile = Path.Combine(Path.GetTempPath(), $"whiskrio_{DateTime.Now:yyyyMMdd_HHmmss}.wav");

        _waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(44100, 1)
        };
        _writer = new WaveFileWriter(_currentFile, _waveIn.WaveFormat);

        _waveIn.DataAvailable += (_, args) =>
        {
            _writer?.Write(args.Buffer, 0, args.BytesRecorded);
            _writer?.Flush();
        };

        _waveIn.StartRecording();
    }

    public Task<string> StopRecordingAsync()
    {
        if (_waveIn is null || _currentFile is null)
        {
            return Task.FromResult(string.Empty);
        }

        var tcs = new TaskCompletionSource<string>();
        _waveIn.RecordingStopped += (_, _) =>
        {
            _writer?.Dispose();
            _writer = null;
            _waveIn?.Dispose();
            _waveIn = null;
            tcs.SetResult(_currentFile);
        };
        _waveIn.StopRecording();

        return tcs.Task;
    }

    public void Dispose()
    {
        _writer?.Dispose();
        _waveIn?.Dispose();
    }
}
