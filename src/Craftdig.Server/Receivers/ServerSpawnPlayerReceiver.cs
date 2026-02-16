namespace Craftdig.Server;

[Server]
public class ServerSpawnPlayerReceiver(AppLog log, WorldDimensionBag dimensionBag)
{
    public void Receive(NetSocket ns)
    {
        if (ns.Ent.SocketPlayer() != null)
        {
            log.Warn("Player {0} tried to spawn again", ns.Ent.Tag());
            ns.Disconnect();
            return;
        }

        log.Info("Player {0} requested to spawn", ns.Ent.Tag());

        var dimensionScope = dimensionBag.Ents[0].DimensionScope();
        ns.Ent.DimensionScope() = dimensionScope;
        ns.Ent.SocketPlayer() = new EntObj();
        ns.Ent.SocketPlayer().Tag(ns.Ent.Tag());
        dimensionScope.Get<DimensionPlayerSpawner>().Add(ns);
    }
}
