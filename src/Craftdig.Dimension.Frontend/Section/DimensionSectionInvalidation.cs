namespace Craftdig.Dimension.Frontend;

[Dimension]
public class DimensionSectionInvalidation(
    DimensionChunks chunks,
    DimensionBlockChanges blockChanges,
    DimensionLightChanges lightChanges)
{
    public void Frame()
    {
        foreach (var c in blockChanges.Span)
            Process(c.Loc);

        foreach (var sloc in lightChanges)
            DirtySection(sloc);
    }

    private void Process(Vec3i loc)
    {
        Dirty(new(0, 0, 0));
        Dirty(new(1, 0, 0));
        Dirty(new(-1, 0, 0));
        Dirty(new(0, 1, 0));
        Dirty(new(0, -1, 0));
        Dirty(new(0, 0, 1));
        Dirty(new(0, 0, -1));

        void Dirty(Vec3i delta)
        {
            var sloc = (loc + delta).ToSloc();
            DirtySection(sloc);
        }
    }

    private void DirtySection(Vec3i sloc)
    {
        if ((uint)sloc.Z >= SectionHeight || !chunks.TryGet(sloc.XY, out var chunk) || !chunk.IsReadyToRender)
            return;

        if (!chunk.Sections.IsEmpty)
        {
            var section = chunk.Sections.Span[sloc.Z];
            if (section.IsMeshPending)
            {
                section.IsMeshDirty = true;
                return;
            }
        }

        if (!chunk.Unrendered.ContainsKey(sloc.Z))
            chunk.Unrendered.Add(sloc.Z, sloc.Z);
    }
}
