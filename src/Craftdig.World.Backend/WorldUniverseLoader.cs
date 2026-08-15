namespace Craftdig;

[WorldLoader]
public class WorldUniverseLoader(
    WorldUniverse universe,
    WorldEntArena entArena)
{
    public void Run(EntRegionState region)
    {
        foreach (var ent in region.Ents)
        {
            if (!ent.IsUniverse)
                continue;

            universe.Bind(ent);
            return;
        }

        universe.Bind(entArena.Alloc().Mutate()
            .IsUniverse(true)
            .Ent);
    }
}
