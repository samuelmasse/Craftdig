namespace Craftdig.Menus.Multiplayer;

public record ServerPingResult
{
    public bool Success { get; init; }
    public TimeSpan? Ping { get; init; }
    public int? MaxPlayers { get; init; }
    public int? CurrentPlayers { get; init; }
    public string? Description { get; init; }
    public byte[]? IconData { get; init; }
}
