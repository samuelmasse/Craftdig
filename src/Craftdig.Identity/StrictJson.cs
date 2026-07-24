namespace Craftdig.Identity;

public static class StrictJson
{
    public static JsonDocumentOptions DocumentOptions => new()
    {
        AllowTrailingCommas = false,
        CommentHandling = JsonCommentHandling.Disallow,
        MaxDepth = 8,
    };

    public static bool HasPropertyCount(JsonElement value, int expected)
    {
        if (value.ValueKind != JsonValueKind.Object)
            return false;

        int count = 0;
        foreach (var _ in value.EnumerateObject())
            count++;
        return count == expected;
    }

    public static bool TryGetUnique(JsonElement parent, string name, out JsonElement value)
    {
        value = default;
        bool found = false;
        if (parent.ValueKind != JsonValueKind.Object)
            return false;

        foreach (var property in parent.EnumerateObject())
        {
            if (property.NameEquals(name))
            {
                if (found)
                    return false;

                found = true;
                value = property.Value;
            }
        }

        return found;
    }

    public static bool TryGetString(JsonElement parent, string name, [NotNullWhen(true)] out string? value)
    {
        value = null;
        if (!TryGetUnique(parent, name, out var element) || element.ValueKind != JsonValueKind.String)
            return false;

        value = element.GetString();
        return value != null;
    }

    public static bool TryGetCanonicalInt64(JsonElement parent, string name, out long value)
    {
        value = default;
        if (!TryGetUnique(parent, name, out var element) || element.ValueKind != JsonValueKind.Number)
            return false;

        string raw = element.GetRawText();
        return raw.Length > 0 && raw[0] != '0' &&
            long.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out value);
    }

    public static bool TryGetCanonicalUuid(JsonElement parent, string name, out Guid value)
    {
        value = default;
        return TryGetString(parent, name, out string? text) && IsCanonicalUuid(text, out value);
    }

    public static bool IsCanonicalUuid(string text, out Guid value) =>
        Guid.TryParseExact(text, "D", out value) && value != Guid.Empty && value.ToString("D") == text;

    public static bool TryDecodeCanonicalBase64Url(string encoded, [NotNullWhen(true)] out byte[]? decoded)
    {
        decoded = null;
        try
        {
            decoded = Base64UrlEncoder.DecodeBytes(encoded);
        }
        catch (FormatException)
        {
            return false;
        }

        if (Base64UrlEncoder.Encode(decoded) == encoded)
            return true;

        decoded = null;
        return false;
    }
}
