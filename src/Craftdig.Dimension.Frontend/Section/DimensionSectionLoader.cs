namespace Craftdig.Dimension.Frontend;

[Dimension]
public class DimensionSectionLoader(DimensionSectionThreadWorkQueue sectionThreadWorkQueue)
{
    public void Load(EntMut section)
    {
        sectionThreadWorkQueue.Enqeue(section.Sloc());
        section.Chunk().Unrendered().Remove(section.Sloc().Z);
    }
}
