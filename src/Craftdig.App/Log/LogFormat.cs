namespace Craftdig.App;

[App]
public static class LogFormat
{
    public static readonly string Fatal = "FATAL";
    public static readonly string Error = "ERROR";
    public static readonly string Warn = "WARN";
    public static readonly string Info = "INFO";
    public static readonly string Debug = "DEBUG";
    public static readonly string Trace = "TRACE";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void StartFatal(string? file, int? line, ref Utf16ValueStringBuilder sb, out DateTime time) =>
        Start(Fatal, file, line, ref sb, out time);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void StartError(string? file, int? line, ref Utf16ValueStringBuilder sb, out DateTime time) =>
        Start(Error, file, line, ref sb, out time);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void StartWarn(string? file, int? line, ref Utf16ValueStringBuilder sb, out DateTime time) =>
        Start(Warn, file, line, ref sb, out time);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void StartInfo(string? file, int? line, ref Utf16ValueStringBuilder sb, out DateTime time) =>
        Start(Info, file, line, ref sb, out time);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void StartDebug(string? file, int? line, ref Utf16ValueStringBuilder sb, out DateTime time) =>
        Start(Debug, file, line, ref sb, out time);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void StartTrace(string? file, int? line, ref Utf16ValueStringBuilder sb, out DateTime time) =>
        Start(Trace, file, line, ref sb, out time);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Start(string? type, string? file, int? line, ref Utf16ValueStringBuilder sb, out DateTime time)
    {
        time = DateTime.UtcNow;

        AppendTimestamp(ref sb, time);

        if (type != null)
        {
            sb.Append('[');
            sb.Append(type);
            sb.Append("] ");
        }

        if (file != null)
        {
            int index = file.LastIndexOf('\\') + 1;
            sb.Append('[');
            sb.Append(file, index, file.Length - index);
            if (line != null)
            {
                sb.Append(':');
                sb.Append(line.Value);
            }
            sb.Append("] ");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Exception(ref Utf16ValueStringBuilder sb, Exception exception)
    {
        sb.Append(Environment.NewLine);
        sb.Append(exception.ToString());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendTimestamp(ref Utf16ValueStringBuilder sb, DateTime time)
    {
        Span<char> buf = stackalloc char[27];
        "[0000-00-00T00:00:00.000Z] ".AsSpan().CopyTo(buf);

        Write4Digits(buf, 1, time.Year);
        Write2Digits(buf, 6, time.Month);
        Write2Digits(buf, 9, time.Day);
        Write2Digits(buf, 12, time.Hour);
        Write2Digits(buf, 15, time.Minute);
        Write2Digits(buf, 18, time.Second);
        Write3Digits(buf, 21, time.Millisecond);

        sb.Append(buf);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Write2Digits(Span<char> buf, int index, int value)
    {
        int tens = value / 10;
        int ones = value - tens * 10;
        buf[index + 0] = (char)('0' + tens);
        buf[index + 1] = (char)('0' + ones);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Write3Digits(Span<char> buf, int index, int value)
    {
        int hundreds = value / 100;
        int rem = value - hundreds * 100;
        int tens = rem / 10;
        int ones = rem - tens * 10;

        buf[index + 0] = (char)('0' + hundreds);
        buf[index + 1] = (char)('0' + tens);
        buf[index + 2] = (char)('0' + ones);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Write4Digits(Span<char> buf, int index, int value)
    {
        int thousands = value / 1000;
        int rem = value - thousands * 1000;
        int hundreds = rem / 100;
        rem -= hundreds * 100;
        int tens = rem / 10;
        int ones = rem - tens * 10;

        buf[index + 0] = (char)('0' + thousands);
        buf[index + 1] = (char)('0' + hundreds);
        buf[index + 2] = (char)('0' + tens);
        buf[index + 3] = (char)('0' + ones);
    }
}
