namespace Craftdig;

[Server]
public class ServerSpawnPlayerReceiver(
    WorldDimensionBag dimensionBag,
    ServerPlayerProfileIds profileIds)
{
    public void Receive(NetSocket ns) =>
        dimensionBag.Ents[0].DimensionScope.Get<DimensionPlayerSpawner>().Add(ns, profileIds.Get(ns.AuthenticatedUid!));
}
