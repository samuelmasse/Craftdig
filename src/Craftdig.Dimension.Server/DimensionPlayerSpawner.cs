namespace Craftdig.Dimension.Server;

[Dimension]
public class DimensionPlayerSpawner(
    WorldIndicesWrapper indicesWrapper,
    WorldPlayerSockets playerSockets,
    WorldPlayerSlots playerSlots,
    DimensionScope scope,
    DimensionEntArena entArena,
    DimensionSockets sockets)
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
        var player = entArena.Alloc();
        player.Tag = ns.Tag;
        player.IsRigid = true;
        player.IsPlayer = true;
        player.HitBox = new Box3d((-0.3, -0.3, -1.62), (0.3, 0.3, 0.18));
        player.Position = (15, 0, 120);
        player.IsFlying = true;
        player.CanFly = true;
        player.CanMove = true;
        player.CanSprint = true;
        player.CanJump = true;
        player.IsSeer = true;
        player.IsLoaded = true;

        sockets.Add(ns);
        ns.DimensionScope = scope;
        ns.SocketPlayer = player;

        playerSockets.Add(ns);
        ns.PlayerSlot = playerSlots.Take();
        ns.Send<WorldIndicesUpdateCommand, byte>(indicesWrapper.Wrap());
    }
}
