namespace Craftdig.Dimension.Frontend;

[Dimension]
public class DimensionSections(DimensionChunks chunks, DimensionEntArena entArena)
{
    private readonly Queue<Memory<EntPtrIdx>> pool = [];

    public bool TryGet(Vector3i sloc, out EntMutIdx ent)
    {
        if (!chunks.TryGet(sloc.Xy, out var chunk))
        {
            ent = default;
            return false;
        }

        if (chunk.Sections.IsEmpty)
        {
            chunk.Sections = pool.Count > 0 ? pool.Dequeue() : new EntPtrIdx[SectionHeight];

            for (int z = 0; z < chunk.Sections.Length; z++)
            {
                chunk.Sections.Span[z] = entArena.Alloc().Mutate()
                    .IsSection(true)
                    .Chunk(chunk)
                    .Sloc((sloc.X, sloc.Y, z));
            }
        }

        ent = chunk.Sections.Span[sloc.Z];
        return true;
    }

    public void ReturnSections(Memory<EntPtrIdx> sections)
    {
        sections.Span.Clear();
        pool.Enqueue(sections);
    }
}
