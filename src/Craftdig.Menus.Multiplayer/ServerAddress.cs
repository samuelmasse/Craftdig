namespace Craftdig.Menus.Multiplayer;

public record ServerAddress(string Host, int Port)
{
    public static ServerAddress Parse(string address, int defaultPort = 36676)
    {
        var colonIdx = address.LastIndexOf(':');
        if (colonIdx >= 0 && int.TryParse(address.AsSpan()[(colonIdx + 1)..], out var port))
            return new(address[..colonIdx], port);

        return new(address, defaultPort);
    }
}
