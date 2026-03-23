namespace Craftdig.Dimension.Server;

[Dimension]
public class DimensionPlayerSpawner(
    DimensionScope scope,
    DimensionEntArena entArena,
    DimensionSockets sockets)
{
    public void Add(NetSocket ns)
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
    }
}
