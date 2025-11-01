namespace Crafthoe.World;

[WorldLoader]
public class WorldUnloader(WorldEntPtrBag entPtrBag)
{
    public void Run()
    {
        foreach (var ent in entPtrBag.Ents)
            ent.Dispose();
    }
}
