namespace Craftdig.Menus.Multiplayer;

public record ServerEntry(string Name, string Host, int Port)
{
    public static (string Host, int Port) ParseAddress(string address, int defaultPort = 36676)
    {
        var colonIdx = address.LastIndexOf(':');
        if (colonIdx >= 0 && int.TryParse(address.AsSpan()[(colonIdx + 1)..], out var port))
            return (address[..colonIdx], port);

        return (address, defaultPort);
    }
}
