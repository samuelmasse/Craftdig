namespace Craftdig;

[Server]
public class ServerSpawnPlayerReceiver(
    WorldDimensionBag dimensionBag,
    ServerPlayerProfileIds profileIds)
{
    public void Receive(NetSocket ns)
    {
        var spawner = dimensionBag.Ents[0].DimensionScope.Get<DimensionPlayerSpawner>();
        var developmentName = ns.IdentitySession == null ? Encoding.ASCII.GetBytes(ns.AuthenticatedUsername!) : null;
        spawner.Add(ns, profileIds.Get(ns.AuthenticatedUid!), developmentName);
    }
}
