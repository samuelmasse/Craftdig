namespace Crafthoe.Server;

[World]
public class WorldSpawnPlayerReceiver(WorldDimensionBag dimensionBag)
{
    public void Receive(NetSocket ns, NetMessage msg)
    {
        if (ns.Ent.SocketPlayer() != null)
            return;

        Console.WriteLine("Was asked to spawn");

        var dimensionScope = dimensionBag.Ents[0].DimensionScope();
        ns.Ent.DimensionScope() = dimensionScope;
        ns.Ent.SocketPlayer() = new EntObj();
        dimensionScope.Get<DimensionPlayerSpawner>().Add(ns);
    }
}
