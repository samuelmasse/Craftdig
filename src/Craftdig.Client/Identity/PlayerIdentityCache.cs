namespace Craftdig;

[Player]
public class PlayerIdentityCache
{
    private readonly Lock entGate = new();
    private readonly Dictionary<Guid, string?> playerEnts = [];
    private IReadOnlyDictionary<Guid, PlayerIdentitySnapshot> players =
        FrozenDictionary<Guid, PlayerIdentitySnapshot>.Empty;
    private int playerListOpen;

    public IReadOnlyDictionary<Guid, PlayerIdentitySnapshot> Players => Volatile.Read(ref players);
    public bool IsPlayerListOpen => Volatile.Read(ref playerListOpen) != 0;

    public bool TryGet(Guid playerId, [NotNullWhen(true)] out PlayerIdentitySnapshot? snapshot) =>
        Players.TryGetValue(playerId, out snapshot);

    public void ObservePlayerEnt(Guid playerId, ReadOnlySpan<byte> developmentName = default)
    {
        lock (entGate)
        {
            if (playerEnts.TryGetValue(playerId, out var previousName))
            {
                if (Ascii.Equals(developmentName, previousName.AsSpan()))
                    return;
            }
            else if (playerEnts.Count >= ProtocolLimits.MaxPresencePlayers)
                return;

            playerEnts[playerId] = DevelopmentPlayerName.Decode(developmentName);
        }
    }

    public void ForgetPlayerEnt(Guid playerId)
    {
        lock (entGate)
            playerEnts.Remove(playerId);
    }

    public bool IsEntPresent(Guid playerId) => TryGetPlayerEnt(playerId, out _);

    public bool TryGetPlayerEnt(Guid playerId, out string? developmentName)
    {
        lock (entGate)
            return playerEnts.TryGetValue(playerId, out developmentName);
    }

    public Guid[] CaptureEnts()
    {
        lock (entGate)
            return [.. playerEnts.Keys];
    }

    public void SetPlayerListOpen(bool open) => Volatile.Write(ref playerListOpen, open ? 1 : 0);

    public void Publish(IReadOnlyDictionary<Guid, PlayerIdentitySnapshot> snapshots) =>
        Volatile.Write(ref players, snapshots.ToFrozenDictionary());
}
