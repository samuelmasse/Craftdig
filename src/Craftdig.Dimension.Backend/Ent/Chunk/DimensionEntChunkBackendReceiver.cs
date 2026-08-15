namespace Craftdig;

[Dimension]
public class DimensionEntChunkBackendReceiver(
    DimensionChunks chunks,
    DimensionChunkRigids chunkRigids,
    DimensionEntRegionStates regions)
{
    public void Receive(EntMutIdx chunk)
    {
        var cloc = chunk.Cloc;
        Process(cloc);
        Process(cloc + (1, 0));
        Process(cloc + (0, 1));
        Process(cloc + (-1, 0));
        Process(cloc + (0, -1));
        Process(cloc + (1, 1));
        Process(cloc + (-1, 1));
        Process(cloc + (-1, -1));
        Process(cloc + (1, -1));
    }

    private void Process(Vec2i cloc)
    {
        if (IsNull(cloc + (1, 0)) ||
            IsNull(cloc + (0, 1)) ||
            IsNull(cloc + (-1, 0)) ||
            IsNull(cloc + (0, -1)) ||
            IsNull(cloc + (1, 1)) ||
            IsNull(cloc + (-1, 1)) ||
            IsNull(cloc + (-1, -1)) ||
            IsNull(cloc + (1, -1)))
            return;

        if (!chunks.TryGet(cloc, out var chunk))
            return;

        if (!chunk.IsChunkComponentsLoaded)
        {
            var rloc = chunk.Cloc.ToRloc();
            regions.EnsureLoaded(rloc);

            foreach (var ent in chunkRigids[chunk.Cloc])
            {
                var ecloc = ent.Position.ToLoc().Xy.ToCloc();
                if (!ent.IsPlayer && ecloc == cloc)
                    ent.Mutate().IsLoaded(true);
            }

            chunk.IsChunkComponentsLoaded = true;
        }
    }

    private bool IsNull(Vec2i cloc) => !chunks.TryGet(cloc, out _);
}
