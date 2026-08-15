namespace Craftdig;

public record ServerAddress(string Host, int Port)
{
    public const int DefaultPort = 36676;

    public static ServerAddress Parse(string address, int defaultPort = DefaultPort)
    {
        if (address.StartsWith('['))
        {
            int closingBracket = address.IndexOf(']');
            if (closingBracket > 1)
            {
                string host = address[1..closingBracket];
                if (closingBracket == address.Length - 1)
                    return new(host, defaultPort);
                if (address[closingBracket + 1] == ':' &&
                    int.TryParse(address.AsSpan()[(closingBracket + 2)..], out int bracketedPort))
                    return new(host, bracketedPort);
            }
        }

        if (IPAddress.TryParse(address, out var ipAddress) && ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
            return new(ipAddress.ToString(), defaultPort);

        var colonIdx = address.LastIndexOf(':');
        if (colonIdx >= 0 && int.TryParse(address.AsSpan()[(colonIdx + 1)..], out var port))
            return new(address[..colonIdx], port);

        return new(address, defaultPort);
    }

    public override string ToString()
    {
        string formattedHost = Host.Contains(':') ? $"[{Host}]" : Host;
        return Port == DefaultPort ? formattedHost : $"{formattedHost}:{Port}";
    }
}
