namespace Crafthoe.Dimension;

[Dimension]
public class DimensionSections(DimensionChunks chunks)
{
    public bool TryGet(Vector3i sloc, out Entity entity)
    {
        var chunk = chunks[sloc.Xy];
        if (chunk == null)
        {
            entity = default;
            return false;
        }

        var sections = chunk.Value.ChunkSections();
        if (sections.IsEmpty)
        {
            // TODO: the entities must be cleaned up
            var array = new Entity[HeightSize / SectionSize];
            for (int z = 0; z < array.Length; z++)
                array[z] = new Entity().IsSection(true).SectionLocation((sloc.X, sloc.Y, z));

            chunk.Value.ChunkSections(array);
            sections = chunk.Value.ChunkSections();
        }

        entity = sections[sloc.Z];
        return true;
    }
}
