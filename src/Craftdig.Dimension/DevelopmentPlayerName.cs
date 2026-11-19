namespace Craftdig;

public static class DevelopmentPlayerName
{
    public const int MaxBytes = 35;

    public static string? Decode(ReadOnlySpan<byte> data)
    {
        if (data.Length is < 1 or > MaxBytes || data.ContainsAnyExceptInRange((byte)0x21, (byte)0x7e))
            return null;

        return Encoding.ASCII.GetString(data);
    }
}
