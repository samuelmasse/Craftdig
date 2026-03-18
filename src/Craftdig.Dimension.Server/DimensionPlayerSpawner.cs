namespace Craftdig.Dimension.Server;

[Dimension]
public class DimensionPlayerSpawner(
    WorldIndicesWrapper indicesWrapper,
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
        var player = ns.SocketPlayer;
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

        ns.Send<WorldIndicesUpdateCommand, byte>(indicesWrapper.Wrap());
    }
}
