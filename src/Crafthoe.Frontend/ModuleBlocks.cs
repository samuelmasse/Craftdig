namespace Crafthoe.Frontend;

[Module]
public class ModuleBlocks
{
    public readonly Ent Air;
    public readonly Ent Grass;
    public readonly Ent Dirt;
    public readonly Ent Stone;

    public ModuleBlocks(ModuleEnts entities, ModuleBlockFaces faces)
    {
        Air = entities["AirBlock"]
            .Name("Air")
            .IsBlock(true);

        Grass = entities["GrassBlock"]
            .Name("Grass")
            .MaxStack(64)
            .IsBlock(true)
            .IsSolid(true)
            .IsBuildable(true)
            .Faces(new()
            {
                Top = faces.Grass,
                Bottom = faces.Dirt,
                Left = faces.GrassSide,
                Right = faces.GrassSide,
                Front = faces.GrassSide,
                Back = faces.GrassSide
            });

        Dirt = entities["DirtBlock"]
            .Name("Dirt")
            .MaxStack(64)
            .IsBlock(true)
            .IsSolid(true)
            .IsBuildable(true)
            .Faces(new(faces.Dirt));

        Stone = entities["StoneBlock"]
            .Name("Stone")
            .MaxStack(64)
            .IsBlock(true)
            .IsSolid(true)
            .IsBuildable(true)
            .Faces(new(faces.Stone));
    }
}
