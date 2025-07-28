namespace Crafthoe.Module;

[Module]
public class ModuleBlocks
{
    public readonly ReadOnlyEntity Air;
    public readonly ReadOnlyEntity Grass;
    public readonly ReadOnlyEntity Dirt;
    public readonly ReadOnlyEntity Stone;

    public ModuleBlocks()
    {
        Air = new Entity()
            .ModuleId(1)
            .ModuleName("Air");

        Grass = new Entity()
            .ModuleId(2)
            .ModuleName("Grass")
            .IsSolid(true);

        Dirt = new Entity()
            .ModuleId(3)
            .ModuleName("Dirt")
            .IsSolid(true);

        Stone = new Entity()
            .ModuleId(4)
            .ModuleName("Stone")
            .IsSolid(true);
    }
}
