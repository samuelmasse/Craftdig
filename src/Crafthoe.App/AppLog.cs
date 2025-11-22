namespace Crafthoe.App;

[App]
public class AppLog
{
    public void Raw(string msg)
    {
        Open(out var sb);
        sb.Append(msg);
        Print(sb);
        Close(sb);
    }

    public void Info(string format, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        StartInfo(file, line, out var sb);
        sb.Append(format);
        End(sb);
    }

    public void Info<T>(T arg, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        StartInfo(file, line, out var sb);
        sb.Append(arg);
        End(sb);
    }

    public void Info<T>(string format, T arg, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        StartInfo(file, line, out var sb);
        sb.AppendFormat(format, arg);
        End(sb);
    }

    public void Info<T1, T2>(string format, T1 arg1, T2 arg2, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        StartInfo(file, line, out var sb);
        sb.AppendFormat(format, arg1, arg2);
        End(sb);
    }

    private void End(Utf16ValueStringBuilder sb)
    {
        Print(sb);
        Close(sb);
    }

    private void Print(Utf16ValueStringBuilder sb) =>
        Console.Out.WriteLine(sb.AsSpan());

    private void StartInfo(string file, int line, out Utf16ValueStringBuilder sb) =>
        Start("INFO", file, line, out sb);

    private void Start(string type, string file, int line, out Utf16ValueStringBuilder sb)
    {
        sb = ZString.CreateStringBuilder();
        var time = DateTime.UtcNow;
        sb.AppendFormat("[{0:0000}-{1:00}-{2:00}T{3:00}:{4:00}:{5:00}.{6:000}Z] ",
            time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second, time.Millisecond);
        sb.AppendFormat("[{0}] ", type);

        int index = file.LastIndexOf('\\') + 1;
        sb.Append('[');
        sb.Append(file, index, file.Length - index);
        sb.AppendFormat(":{0}] ", line);
    }

    private void Open(out Utf16ValueStringBuilder sb) => sb = ZString.CreateStringBuilder();
    private void Close(Utf16ValueStringBuilder sb) => sb.Dispose();
}
