namespace Craftdig.World;

[WorldLoader]
public class WorldUnloader(WorldEntArena entArena)
{
    public void Run()
    {
        entArena.Dispose();
    }
}
