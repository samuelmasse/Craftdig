namespace Craftdig;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct RosterPing(Guid PlayerId, ushort Milliseconds)
{
    public static readonly TimeSpan MaxAge = TimeSpan.FromSeconds(10);
}
