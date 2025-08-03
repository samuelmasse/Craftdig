namespace Crafthoe.Dimension;

[Dimension]
public class DimensionSections(DimensionChunks chunks)
{
    private readonly Queue<Memory<EntPtr>> pool = [];

    public bool TryGet(Vector3i sloc, out EntMut entity)
    {
        if (!chunks.TryGet(sloc.Xy, out var chunk))
        {
            entity = default;
            return false;
        }

        if (chunk.Sections().IsEmpty)
        {
            chunk.Sections() = pool.Count > 0 ? pool.Dequeue() : new EntPtr[HeightSize / SectionSize];

            for (int z = 0; z < chunk.Sections().Length; z++)
                chunk.Sections().Span[z] = new EntPtr().IsSection(true).Chunk(chunk).Sloc((sloc.X, sloc.Y, z));
        }

        entity = chunk.Sections().Span[sloc.Z];
        return true;
    }

    public void ReturnSections(Memory<EntPtr> sections)
    {
        sections.Span.Clear();
        pool.Enqueue(sections);
    }
}
