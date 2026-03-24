namespace Craftdig.Server;

[Server]
public class ServerPlayerSpawner(
    WorldIndicesWrapper indicesWrapper,
    WorldDimensionBag dimensionBag,
    ServerPlayerSockets playerSockets,
    ServerPlayerSlots playerSlots)
{
    private readonly ConcurrentQueue<NetSocket> queue = [];

    public void Add(NetSocket ns)
    {
        queue.Enqueue(ns);
    }

    public void Tick()
    {
        int count = queue.Count;

        while (count > 0 && queue.TryDequeue(out var ns))
        {
            Spawn(ns);
            count--;
        }
    }

    private void Spawn(NetSocket ns)
    {
        dimensionBag.Ents[0].DimensionScope.Get<DimensionPlayerSpawner>().Add(ns);
        playerSockets.Add(ns);
        ns.PlayerSlot = playerSlots.Take();
        ns.Send<WorldIndicesUpdateCommand, byte>(indicesWrapper.Wrap());
    }
}
