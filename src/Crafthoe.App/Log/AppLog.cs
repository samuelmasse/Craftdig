namespace Crafthoe.App;

[App]
public partial class AppLog(AppLogStream logStream)
{
    private LogLevel level = LogLevel.Info;

    public ref LogLevel Level => ref level;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Raw(string msg,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        var sb = Open();
        sb.Append(msg);
        End(file, line, DateTime.UtcNow, sb, LogLevel.None);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void End(string file, int line, DateTime time, Utf16ValueStringBuilder sb, LogLevel level)
    {
        logStream.Add(new(time, level, file, line), sb.AsSpan());
        sb.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Utf16ValueStringBuilder Open() => ZString.CreateStringBuilder(true);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AppendException(ref Utf16ValueStringBuilder sb, Exception? exception)
    {
        if (exception != null)
            LogFormat.Exception(ref sb, exception);
    }
}
