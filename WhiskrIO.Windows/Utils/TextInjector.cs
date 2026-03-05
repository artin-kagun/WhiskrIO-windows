namespace WhiskrIO.Windows.Utils;

public static class TextInjector
{
    public static void InjectText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        SendKeys.SendWait(text);
    }
}
