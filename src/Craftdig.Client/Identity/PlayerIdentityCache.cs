namespace Craftdig;

[Player]
public class PlayerIdentityCache
{
    private readonly Lock entGate = new();
    private readonly HashSet<Guid> playerEnts = [];
    private IReadOnlyDictionary<Guid, PlayerIdentitySnapshot> players =
        FrozenDictionary<Guid, PlayerIdentitySnapshot>.Empty;
    private int playerListOpen;

    public IReadOnlyDictionary<Guid, PlayerIdentitySnapshot> Players => Volatile.Read(ref players);
    public bool IsPlayerListOpen => Volatile.Read(ref playerListOpen) != 0;

    public bool TryGet(Guid playerId, [NotNullWhen(true)] out PlayerIdentitySnapshot? snapshot) =>
        Players.TryGetValue(playerId, out snapshot);

    public void ObservePlayerEnt(Guid playerId)
    {
        lock (entGate)
        {
            if (playerEnts.Count < ProtocolLimits.MaxPresencePlayers)
                playerEnts.Add(playerId);
        }
    }

    public void ForgetPlayerEnt(Guid playerId)
    {
        lock (entGate)
            playerEnts.Remove(playerId);
    }

    public bool IsEntPresent(Guid playerId)
    {
        lock (entGate)
            return playerEnts.Contains(playerId);
    }

    public Guid[] CaptureEnts()
    {
        lock (entGate)
            return [.. playerEnts];
    }

    public void SetPlayerListOpen(bool open) => Volatile.Write(ref playerListOpen, open ? 1 : 0);

    public void Publish(IReadOnlyDictionary<Guid, PlayerIdentitySnapshot> snapshots) =>
        Volatile.Write(ref players, snapshots.ToFrozenDictionary());
}
