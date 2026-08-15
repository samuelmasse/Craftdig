namespace Craftdig;

public sealed class ServerContext : IEquatable<ServerContext>
{
    private static ReadOnlySpan<byte> DomainSeparator => "Craftdig Server Context v1\0"u8;

    private readonly byte[] hostBytes;

    private ServerContext(ServerHostKind hostKind, string host, ushort port, byte[] hostBytes)
    {
        HostKind = hostKind;
        Host = host;
        Port = port;
        this.hostBytes = hostBytes;
    }

    public readonly ServerHostKind HostKind;
    public readonly string Host;
    public readonly ushort Port;

    public int EncodedSize => DomainSeparator.Length + sizeof(byte) + sizeof(ushort) + hostBytes.Length + sizeof(ushort);

    public static bool TryCreate(string host, int port, [NotNullWhen(true)] out ServerContext? context)
    {
        context = null;
        if (port is < 1 or > ushort.MaxValue || string.IsNullOrEmpty(host) || host != host.Trim())
            return false;

        if (host.IndexOfAny(['/', '\\', '?', '#', '@', '[', ']', '%']) >= 0)
            return false;

        if (IPAddress.TryParse(host, out var address))
        {
            if (address.IsIPv4MappedToIPv6)
                return false;

            var hostKind = address.AddressFamily switch
            {
                AddressFamily.InterNetwork => ServerHostKind.IPv4,
                AddressFamily.InterNetworkV6 => ServerHostKind.IPv6,
                _ => (ServerHostKind)0,
            };

            if (hostKind == 0)
                return false;

            context = new(hostKind, address.ToString(), (ushort)port, address.GetAddressBytes());
            return true;
        }

        if (!TryCanonicalizeDns(host, out string? canonicalHost))
            return false;

        context = new(ServerHostKind.Dns, canonicalHost, (ushort)port, Encoding.ASCII.GetBytes(canonicalHost));
        return true;
    }

    public static bool TryParseCanonical(string host, int port, [NotNullWhen(true)] out ServerContext? context)
    {
        if (!TryCreate(host, port, out context))
            return false;

        if (string.Equals(host, context.Host, StringComparison.Ordinal))
            return true;

        context = null;
        return false;
    }

    public bool TryWriteCanonical(Span<byte> destination, out int written)
    {
        written = 0;
        if (destination.Length < EncodedSize)
            return false;

        DomainSeparator.CopyTo(destination);
        int offset = DomainSeparator.Length;
        destination[offset++] = (byte)HostKind;
        BinaryPrimitives.WriteUInt16BigEndian(destination[offset..], (ushort)hostBytes.Length);
        offset += sizeof(ushort);
        hostBytes.CopyTo(destination[offset..]);
        offset += hostBytes.Length;
        BinaryPrimitives.WriteUInt16BigEndian(destination[offset..], Port);
        written = offset + sizeof(ushort);
        return true;
    }

    public Hash256 ComputeHash()
    {
        Span<byte> canonical = stackalloc byte[EncodedSize];
        TryWriteCanonical(canonical, out int written);
        return Hash256.Compute(canonical[..written]);
    }

    public override string ToString() => HostKind == ServerHostKind.IPv6 ? $"[{Host}]:{Port}" : $"{Host}:{Port}";

    public bool Equals(ServerContext? other) =>
        other != null &&
        HostKind == other.HostKind &&
        Port == other.Port &&
        string.Equals(Host, other.Host, StringComparison.Ordinal);

    public override bool Equals(object? obj) => Equals(obj as ServerContext);
    public override int GetHashCode() => HashCode.Combine(HostKind, Port, string.GetHashCode(Host, StringComparison.Ordinal));

    public static bool operator ==(ServerContext? left, ServerContext? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(ServerContext? left, ServerContext? right) => !(left == right);

    private static bool TryCanonicalizeDns(string host, [NotNullWhen(true)] out string? canonicalHost)
    {
        canonicalHost = null;
        if (host[^1] == '.')
            return false;

        string ascii;
        try
        {
            ascii = new IdnMapping().GetAscii(host).ToLowerInvariant();
        }
        catch (ArgumentException)
        {
            return false;
        }

        if (ascii.Length is < 1 or > 253)
            return false;

        int labelStart = 0;
        for (int i = 0; i <= ascii.Length; i++)
        {
            if (i != ascii.Length && ascii[i] != '.')
                continue;

            int labelLength = i - labelStart;
            if (labelLength is < 1 or > 63 || ascii[labelStart] == '-' || ascii[i - 1] == '-')
                return false;

            for (int j = labelStart; j < i; j++)
            {
                char c = ascii[j];
                if (!char.IsAsciiLetterOrDigit(c) && c != '-')
                    return false;
            }

            labelStart = i + 1;
        }

        canonicalHost = ascii;
        return true;
    }
}
