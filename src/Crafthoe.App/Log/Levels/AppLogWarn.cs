namespace Crafthoe.App;

public partial class AppLog
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Warn(Exception exception,
        AppLog? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipWarn())
            return;
        StartWarn(file, line, out var time, out var sb);
        AppendException(ref sb, exception);
        EndWarn(file, line, time, sb);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Warn(string msg,
        Exception? exception = null,
        AppLog? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipWarn())
            return;
        StartWarn(file, line, out var time, out var sb);
        sb.Append(msg);
        AppendException(ref sb, exception);
        EndWarn(file, line, time, sb);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Warn<T>(in T arg,
        Exception? exception = null,
        AppLog? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipWarn())
            return;
        StartWarn(file, line, out var time, out var sb);
        sb.Append(arg);
        AppendException(ref sb, exception);
        EndWarn(file, line, time, sb);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Warn<T>(string format,
        in T arg,
        Exception? exception = null,
        AppLog? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipWarn())
            return;
        StartWarn(file, line, out var time, out var sb);
        sb.AppendFormat(format, arg);
        AppendException(ref sb, exception);
        EndWarn(file, line, time, sb);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Warn<T1, T2>(string format,
        in T1 arg1, in T2 arg2,
        Exception? exception = null,
        AppLog? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipWarn())
            return;
        StartWarn(file, line, out var time, out var sb);
        sb.AppendFormat(format, arg1, arg2);
        AppendException(ref sb, exception);
        EndWarn(file, line, time, sb);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Warn<T1, T2, T3>(string format,
        in T1 arg1, in T2 arg2, in T3 arg3,
        Exception? exception = null,
        AppLog? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipWarn())
            return;
        StartWarn(file, line, out var time, out var sb);
        sb.AppendFormat(format, arg1, arg2, arg3);
        AppendException(ref sb, exception);
        EndWarn(file, line, time, sb);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Warn<T1, T2, T3, T4>(string format,
        in T1 arg1, in T2 arg2, in T3 arg3, in T4 arg4,
        Exception? exception = null,
        AppLog? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipWarn())
            return;
        StartWarn(file, line, out var time, out var sb);
        sb.AppendFormat(format, arg1, arg2, arg3, arg4);
        AppendException(ref sb, exception);
        EndWarn(file, line, time, sb);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Warn<T1, T2, T3, T4, T5>(string format,
        in T1 arg1, in T2 arg2, in T3 arg3, in T4 arg4, in T5 arg5,
        Exception? exception = null,
        AppLog? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipWarn())
            return;
        StartWarn(file, line, out var time, out var sb);
        sb.AppendFormat(format, arg1, arg2, arg3, arg4, arg5);
        AppendException(ref sb, exception);
        EndWarn(file, line, time, sb);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Warn<T1, T2, T3, T4, T5, T6>(string format,
        in T1 arg1, in T2 arg2, in T3 arg3, in T4 arg4, in T5 arg5, in T6 arg6,
        Exception? exception = null,
        AppLog? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipWarn())
            return;
        StartWarn(file, line, out var time, out var sb);
        sb.AppendFormat(format, arg1, arg2, arg3, arg4, arg5, arg6);
        AppendException(ref sb, exception);
        EndWarn(file, line, time, sb);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Warn<T1, T2, T3, T4, T5, T6, T7>(string format,
        in T1 arg1, in T2 arg2, in T3 arg3, in T4 arg4, in T5 arg5, in T6 arg6, in T7 arg7,
        Exception? exception = null,
        AppLog? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipWarn())
            return;
        StartWarn(file, line, out var time, out var sb);
        sb.AppendFormat(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        AppendException(ref sb, exception);
        EndWarn(file, line, time, sb);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Warn<T1, T2, T3, T4, T5, T6, T7, T8>(string format,
        in T1 arg1, in T2 arg2, in T3 arg3, in T4 arg4, in T5 arg5, in T6 arg6, in T7 arg7, in T8 arg8,
        Exception? exception = null,
        AppLog? _ = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (SkipWarn())
            return;
        StartWarn(file, line, out var time, out var sb);
        sb.AppendFormat(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        AppendException(ref sb, exception);
        EndWarn(file, line, time, sb);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool SkipWarn() => level < LogLevel.Warn;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void StartWarn(string file, int line, out DateTime time, out Utf16ValueStringBuilder sb)
    {
        sb = Open();
        LogFormat.StartWarn(file, line, ref sb, out time);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EndWarn(string file, int line, DateTime time, Utf16ValueStringBuilder sb) =>
        End(file, line, time, sb, LogLevel.Warn);
}
