namespace Craftdig.World.Backend;

[WorldLoader]
public class WorldTimeLoader(WorldUniverse universe)
{
    private readonly long initialTime = 6000;

    public void Run()
    {
        if (!universe.Has<long, WorldComponents.Time>())
            universe.Time = initialTime;
    }
}
