namespace Crafthoe.Native;

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
        m.GrassBlock
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
            .Faces(new(m.DirtFace));

        m.StoneBlock
            .Faces(new(m.StoneFace));
    }
}
