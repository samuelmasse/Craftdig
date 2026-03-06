namespace Craftdig.Dimension.Frontend;

[Dimension]
public class DimensionSectionLoader(DimensionSectionThreadWorkQueue sectionThreadWorkQueue)
{
    public void Load(EntMutIdx section)
    {
        sectionThreadWorkQueue.Enqeue(section.Sloc);
        section.Chunk.Unrendered.Remove(section.Sloc.Z);
    }
}
