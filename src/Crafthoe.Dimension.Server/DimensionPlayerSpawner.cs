namespace Crafthoe.Server;

[Dimension]
public class DimensionPlayerSpawner(
    WorldIndicesWrapper indicesWrapper,
    DimensionPlayerBag playerBag,
    DimensionRigidBag rigidBag,
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
        var player = ns.Ent.SocketPlayer();
        player.HitBox() = new Box3d((-0.3, -0.3, -1.62), (0.3, 0.3, 0.18));
        player.Position() = (15, 0, 120);
        playerBag.Add(player);
        rigidBag.Add(player);
        sockets.Add(ns);

        ns.Send(new((int)ClientCommand.WorldIndicesUpdate, indicesWrapper.Wrap()));
    }
}
