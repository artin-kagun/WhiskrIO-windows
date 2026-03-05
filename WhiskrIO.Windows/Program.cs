using WhiskrIO.Windows.Services;

namespace WhiskrIO.Windows;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        using var appContext = new TrayApplicationContext();
        Application.Run(appContext);
    }
}
