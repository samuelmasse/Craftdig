namespace Craftdig.Protocol;

public static class MonotonicTime
{
    public static long DurationTicks(TimeSpan duration) =>
        (long)(duration.TotalSeconds * Stopwatch.Frequency);

    public static TimeSpan ToTimeSpan(long stopwatchTicks) =>
        TimeSpan.FromSeconds((double)stopwatchTicks / Stopwatch.Frequency);
}
