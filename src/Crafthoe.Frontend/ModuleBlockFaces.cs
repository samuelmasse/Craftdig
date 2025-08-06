namespace Crafthoe.Frontend;

[Module]
public class ModuleBlockFaces
{
    public readonly Ent Grass;
    public readonly Ent GrassSide;
    public readonly Ent Stone;
    public readonly Ent Dirt;

    public ModuleBlockFaces(ModuleEnts entities, ModuleBlockAtlas blockAtlas)
    {
        var faces = new List<EntMut>();

        Grass = Face("GrassFace")
            .FaceFile("Grass");

        GrassSide = Face("GrassSideFace")
            .FaceFile("GrassSide");

        Stone = Face("StoneFace")
            .FaceFile("Stone");

        Dirt = Face("DirtFace")
            .FaceFile("Dirt");

        faces.ForEach(x => x.FaceIndex(blockAtlas[x.FaceFile()]));

        EntMut Face(string name)
        {
            var ent = entities[name].IsFace(true);
            faces.Add(ent);
            return ent;
        }
    }
}
