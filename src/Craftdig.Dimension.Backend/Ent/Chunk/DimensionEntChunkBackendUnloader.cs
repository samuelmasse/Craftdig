namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionEntChunkBackendUnloader(
    DimensionChunks chunks,
    DimensionChunkRigids chunkRigids)
{
    public void Unload(EntMut chunk)
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

    private void Process(Vector2i cloc)
    {
        if (!chunks.TryGet(cloc, out var chunk))
            return;

        if (chunk.IsChunkComponentsLoaded)
        {
            foreach (var ent in chunkRigids[chunk.Cloc])
            {
                var ecloc = ent.Position.ToLoc().Xy.ToCloc();
                if (ecloc == cloc)
                    ent.Mutate().IsLoaded(false);
            }

            chunk.IsChunkComponentsLoaded = false;
        }
    }
}
