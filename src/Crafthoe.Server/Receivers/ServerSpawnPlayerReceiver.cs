namespace Crafthoe.Server;

[Server]
public class ServerSpawnPlayerReceiver(AppLog log, WorldDimensionBag dimensionBag)
{
    public void Receive(NetSocket ns)
    {
        if (ns.Ent.SocketPlayer() != null)
        {
            ns.Disconnect();
            return;
        }

        log.Info("Was asked to spawn");

        var dimensionScope = dimensionBag.Ents[0].DimensionScope();
        ns.Ent.DimensionScope() = dimensionScope;
        ns.Ent.SocketPlayer() = new EntObj();
        ns.Ent.SocketPlayer().Tag(ns.Ent.AuthenticatedUid());
        dimensionScope.Get<DimensionPlayerSpawner>().Add(ns);
    }
}
