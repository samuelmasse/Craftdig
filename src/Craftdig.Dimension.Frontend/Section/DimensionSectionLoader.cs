namespace Craftdig.Dimension.Frontend;

[Dimension]
public class DimensionSectionLoader(DimensionSectionThreadWorkQueue sectionThreadWorkQueue)
{
    public void Load(EntMutIdx section)
    {
        if (section.IsMeshPending)
        {
            section.IsMeshDirty = true;
            section.Chunk.Unrendered.Remove(section.Sloc.Z);
            return;
        }

        int revision = section.MeshRevision + 1;
        section.MeshRevision = revision;
        section.IsMeshPending = true;
        section.IsMeshDirty = false;
        sectionThreadWorkQueue.Enqueue(new(section.Sloc, revision));
        section.Chunk.Unrendered.Remove(section.Sloc.Z);
    }
}
