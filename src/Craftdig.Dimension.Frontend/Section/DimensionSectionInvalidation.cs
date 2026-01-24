namespace Craftdig.Dimension.Frontend;

[Dimension]
public class DimensionSectionInvalidation(DimensionChunks chunks, DimensionBlockChanges blockChanges)
{
    public void Frame()
    {
        foreach (var c in blockChanges.Span)
            Process(c.Loc);
    }

    private void Process(Vector3i loc)
    {
        Dirty(new(0, 0, 0));
        Dirty(new(1, 0, 0));
        Dirty(new(-1, 0, 0));
        Dirty(new(0, 1, 0));
        Dirty(new(0, -1, 0));
        Dirty(new(0, 0, 1));
        Dirty(new(0, 0, -1));

        void Dirty(Vector3i delta)
        {
            var sloc = (loc + delta).ToSloc();
            if (chunks.TryGet(sloc.Xy, out var chunk) && chunk.IsReadyToRender() && !chunk.Unrendered().ContainsKey(sloc.Z) &&
                sloc.Z >= 0 && sloc.Z < SectionSize)
                chunk.Unrendered().Add(sloc.Z, sloc.Z);
        }
    }
}
