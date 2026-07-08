namespace Craftdig.World.Server;

[WorldLoader]
public class WorldServerLoader(WorldEntIdxContextBuilder context, WorldScratchedBagMut scratchedBag)
{
    public void Run()
    {
        context.AddBag(scratchedBag);
    }
}
