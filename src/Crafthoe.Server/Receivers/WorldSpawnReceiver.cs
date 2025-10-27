namespace Crafthoe.Server;

[World]
public class WorldSpawnReceiver(WorldDimensionBag dimensionBag)
{
    public void Receive(NetSocket ns, NetMessage msg)
    {
        if (ns.Ent.SocketPlayer() != null)
            return;

        Console.WriteLine("Was asked to spawn");

        ns.Ent.SocketPlayer() = new EntObj();
        dimensionBag.Ents[0].DimensionScope().Get<DimensionPlayerSpawner>().Add(ns);
    }
}
