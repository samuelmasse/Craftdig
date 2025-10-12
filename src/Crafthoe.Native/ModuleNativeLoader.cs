namespace Crafthoe.Native;

[ModuleLoader]
public class ModuleNativeLoader(ModuleNative m) : ModLoader
{
    public override void Load()
    {
        m.OverworldDimension
            .IsDimension(true)
            .Air(m.AirBlock)
            .TerrainGeneratorType(typeof(DimensionNativeTerrainGenerator))
            .BiomeGeneraetorType(typeof(DimensionNativeBiomeGenerator));

        LoadFaces();
        LoadBlocks();
    }

    private void LoadFaces()
    {
        m.GrassFace
            .IsFace(true)
            .FaceFile("Grass");

        m.GrassSideFace
            .IsFace(true)
            .FaceFile("GrassSide");

        m.StoneFace
            .IsFace(true)
            .FaceFile("Stone");

        m.DirtFace
            .IsFace(true)
            .FaceFile("Dirt");
    }

    private void LoadBlocks()
    {
        m.AirBlock
            .Name("Air")
            .IsBlock(true);

        m.GrassBlock
            .Name("Grass")
            .MaxStack(64)
            .IsBlock(true)
            .IsSolid(true)
            .IsBuildable(true)
            .Faces(new()
            {
                Top = m.GrassFace,
                Bottom = m.DirtFace,
                Left = m.GrassSideFace,
                Right = m.GrassSideFace,
                Front = m.GrassSideFace,
                Back = m.GrassSideFace
            });

        m.DirtBlock
            .Name("Dirt")
            .MaxStack(64)
            .IsBlock(true)
            .IsSolid(true)
            .IsBuildable(true)
            .Faces(new(m.DirtFace));

        m.StoneBlock
            .Name("Stone")
            .MaxStack(64)
            .IsBlock(true)
            .IsSolid(true)
            .IsBuildable(true)
            .Faces(new(m.StoneFace));
    }
}
