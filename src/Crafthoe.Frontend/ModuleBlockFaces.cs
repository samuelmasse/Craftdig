namespace Crafthoe.Frontend;

[Module]
public class ModuleBlockFaces
{
    private readonly EntRef grass;
    private readonly EntRef grassSide;
    private readonly EntRef stone;
    private readonly EntRef dirt;

    public Ent Grass => (Ent)grass;
    public Ent GrassSide => (Ent)grassSide;
    public Ent Stone => (Ent)stone;
    public Ent Dirt => (Ent)dirt;

    public ModuleBlockFaces(ModuleBlockAtlas blockAtlas)
    {
        var faces = new List<EntObj>();

        grass = Face()
            .ModuleName("GrassFace")
            .IsFace(true)
            .FaceFile("Grass");

        grassSide = Face()
            .ModuleName("GrassSideFace")
            .IsFace(true)
            .FaceFile("GrassSide");

        stone = Face()
            .ModuleName("StoneFace")
            .IsFace(true)
            .FaceFile("Stone");

        dirt = Face()
            .ModuleName("DirtFace")
            .IsFace(true)
            .FaceFile("Dirt");

        faces.ForEach(x => x.FaceIndex(blockAtlas[x.FaceFile()]));

        EntObj Face()
        {
            var ent = new EntObj();
            faces.Add(ent);
            return ent;
        }
    }
}
