namespace Crafthoe.Module;

[Module]
public class ModuleBlocks
{
    public readonly EntRef Air;
    public readonly EntRef Grass;
    public readonly EntRef Dirt;
    public readonly EntRef Stone;

    public ModuleBlocks()
    {
        Air = new EntObj()
            .ModuleId(1)
            .ModuleName("Air");

        Grass = new EntObj()
            .ModuleId(2)
            .ModuleName("Grass")
            .IsSolid(true);

        Dirt = new EntObj()
            .ModuleId(3)
            .ModuleName("Dirt")
            .IsSolid(true);

        Stone = new EntObj()
            .ModuleId(4)
            .ModuleName("Stone")
            .IsSolid(true);
    }
}
