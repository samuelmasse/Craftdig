namespace Craftdig.Server;

internal static class ServerPresenceTiming
{
    public static readonly long DrainingRoundLifetime = MonotonicTime.DurationTicks(TimeSpan.FromSeconds(15));
    public static readonly long RoundFanoutWindow = MonotonicTime.DurationTicks(TimeSpan.FromSeconds(4));
    public static readonly long ProofFanoutDelay = MonotonicTime.DurationTicks(TimeSpan.FromSeconds(1));
}
