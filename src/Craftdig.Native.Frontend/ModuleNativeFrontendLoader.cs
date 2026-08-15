namespace Craftdig;

[ModuleLoader]
public class ModuleNativeFrontendLoader(ModuleNative m) : ModLoader
{
    public override void Load()
    {
        LoadFaces();
        LoadBlocks();
    }

    private void LoadFaces()
    {
        m.GrassFace.Mutate()
            .IsFace(true)
            .FaceFile("Grass");

        m.GrassSideFace.Mutate()
            .IsFace(true)
            .FaceFile("GrassSide");

        m.StoneFace.Mutate()
            .IsFace(true)
            .FaceFile("Stone");

        m.DirtFace.Mutate()
            .IsFace(true)
            .FaceFile("Dirt");

        m.GlowstoneFace.Mutate()
            .IsFace(true)
            .FaceFile("Glowstone");
    }

    private void LoadBlocks()
    {
        m.GrassBlock.Mutate()
            .Faces(new()
            {
                Top = m.GrassFace,
                Bottom = m.DirtFace,
                Left = m.GrassSideFace,
                Right = m.GrassSideFace,
                Front = m.GrassSideFace,
                Back = m.GrassSideFace
            });

        m.DirtBlock.Mutate()
            .Faces(new(m.DirtFace));

        m.StoneBlock.Mutate()
            .Faces(new(m.StoneFace));

        m.GlowstoneBlock.Mutate()
            .Faces(new(m.GlowstoneFace));
    }
}
