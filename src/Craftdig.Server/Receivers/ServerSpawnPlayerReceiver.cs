namespace Craftdig.Server;

[Server]
public class ServerSpawnPlayerReceiver(AppLog log, WorldDimensionBag dimensionBag)
{
    public void Receive(NetSocket ns)
    {
        if (ns.SocketPlayer != default)
        {
            log.Warn("Player {0} tried to spawn again", ns.Tag);
            ns.Disconnect();
            return;
        }

        log.Info("Player {0} requested to spawn", ns.Tag);

        var dimensionScope = dimensionBag.Ents[0].DimensionScope;
        ns.DimensionScope = dimensionScope;
        var player = dimensionScope.Get<DimensionEntArena>().Alloc();
        player.Tag = ns.Tag;
        ns.SocketPlayer = player;
        dimensionScope.Get<DimensionPlayerSpawner>().Add(ns);
    }
}
