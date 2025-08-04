namespace Crafthoe.Frontend;

[Module]
public class ModuleBlocks
{
    private readonly ModuleBlockFaces faces;

    public readonly EntRef Air;
    public readonly EntRef Grass;
    public readonly EntRef Dirt;
    public readonly EntRef Stone;

    public ModuleBlocks(ModuleBlockFaces faces)
    {
        this.faces = faces;

        Air = new EntObj()
            .ModuleId(1)
            .ModuleName("Air");

        Grass = new EntObj()
            .ModuleId(2)
            .ModuleName("Grass")
            .IsSolid(true)
            .Faces(new()
            {
                Top = faces.Grass,
                Bottom = faces.Dirt,
                Left = faces.GrassSide,
                Right = faces.GrassSide,
                Front = faces.GrassSide,
                Back = faces.GrassSide
            });

        Dirt = new EntObj()
            .ModuleId(3)
            .ModuleName("Dirt")
            .IsSolid(true)
            .Faces(new(faces.Dirt));

        Stone = new EntObj()
            .ModuleId(4)
            .ModuleName("Stone")
            .IsSolid(true)
            .Faces(new(faces.Stone));
    }
}
