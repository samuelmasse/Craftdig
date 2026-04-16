namespace Craftdig.Menus.Multiplayer;

public record ServerAddress(string Host, int Port)
{
    public const int DefaultPort = 36676;

    public static ServerAddress Parse(string address, int defaultPort = DefaultPort)
    {
        var colonIdx = address.LastIndexOf(':');
        if (colonIdx >= 0 && int.TryParse(address.AsSpan()[(colonIdx + 1)..], out var port))
            return new(address[..colonIdx], port);

        return new(address, defaultPort);
    }

    public override string ToString() => Port == DefaultPort ? Host : $"{Host}:{Port}";
}
