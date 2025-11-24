namespace Crafthoe.Server;

[Server]
public class ServerSpawnPlayerReceiver(AppLog log, WorldDimensionBag dimensionBag)
{
    public void Receive(NetSocket ns)
    {
        if (ns.Ent.SocketPlayer() != null)
        {
            log.Warn("Player {0} tried to spawn again", ns.Ent.AuthenticatedEmail());
            ns.Disconnect();
            return;
        }

        log.Info("Player {0} requested to spawn", ns.Ent.AuthenticatedEmail());

        var dimensionScope = dimensionBag.Ents[0].DimensionScope();
        ns.Ent.DimensionScope() = dimensionScope;
        ns.Ent.SocketPlayer() = new EntObj();
        ns.Ent.SocketPlayer().Tag(ns.Ent.AuthenticatedEmail());
        dimensionScope.Get<DimensionPlayerSpawner>().Add(ns);
    }
}
