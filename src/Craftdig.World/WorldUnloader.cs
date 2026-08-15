namespace Craftdig;

[WorldLoader]
public class WorldUnloader(WorldEntArena entArena)
{
    public void Run()
    {
        entArena.Dispose();
    }
}
