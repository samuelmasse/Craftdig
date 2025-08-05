namespace Crafthoe.Frontend;

[Module]
public class ModuleBlocks
{
    public readonly Ent Air;
    public readonly Ent Grass;
    public readonly Ent Dirt;
    public readonly Ent Stone;

    public ModuleBlocks(ModuleEntities entities, ModuleBlockFaces faces)
    {
        Air = entities["AirBlock"]
            .IsBlock(true);

        Grass = entities["GrassBlock"]
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
            .IsBlock(true)
            .IsSolid(true)
            .IsBuildable(true)
            .Faces(new(faces.Dirt));

        Stone = entities["StoneBlock"]
            .IsBlock(true)
            .IsSolid(true)
            .IsBuildable(true)
            .Faces(new(faces.Stone));
    }
}
