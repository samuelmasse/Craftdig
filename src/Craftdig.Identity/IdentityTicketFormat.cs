namespace Craftdig;

public static class IdentityTicketFormat
{
    public const string TicketType = "craftdig-multiplayer-ticket+jwt";
    public const string Issuer = "craftdig-auth";
    public const string Audience = "craftdig:multiplayer:v1";
    public const int Version = 1;

    public static readonly TimeSpan MaximumLifetime = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan ClockSkew = TimeSpan.FromSeconds(30);

    public static bool IsKeyId(string value)
    {
        if (value.Length is < 1 or > 128)
            return false;

        foreach (char character in value)
        {
            if (!char.IsAsciiLetterOrDigit(character) && character is not '-' and not '_' and not '.')
                return false;
        }

        return true;
    }

    public static bool IsUsername(string value)
    {
        if (value.Length is < 4 or > 35 || !char.IsAsciiLetter(value[0]))
            return false;

        foreach (char character in value)
        {
            if (!char.IsAsciiLetterOrDigit(character))
                return false;
        }

        return true;
    }
}
