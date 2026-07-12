namespace Craftdig.Dimension.Frontend;

[Dimension]
public class DimensionChunkRenderScheduler(DimensionChunks chunks, DimensionBlocksRaw blocksRaw)
{
    public void Add(Vec2i cloc)
    {
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
        if (IsUnavailable(cloc + (1, 0)) ||
            IsUnavailable(cloc + (0, 1)) ||
            IsUnavailable(cloc + (-1, 0)) ||
            IsUnavailable(cloc + (0, -1)) ||
            IsUnavailable(cloc + (1, 1)) ||
            IsUnavailable(cloc + (-1, 1)) ||
            IsUnavailable(cloc + (-1, -1)) ||
            IsUnavailable(cloc + (1, -1)))
            return;

        if (!chunks.TryGet(cloc, out var chunk))
            return;

        if (!chunk.IsLightReady)
            return;

        if (!blocksRaw.TryGetChunkBlocks(cloc, out var blocks))
            return;

        if (!chunk.IsUnrenderedListBuilt)
        {
            for (int sz = 0; sz < SectionHeight; sz++)
            {
                if (ShouldMesh(cloc, blocks, sz))
                    chunk.Unrendered.Add(sz, sz);
            }

            chunk.IsUnrenderedListBuilt = true;
        }

        chunk.IsReadyToRender = true;
    }

    private bool ShouldMesh(Vec2i cloc, ChunkBlocks blocks, int sz)
    {
        var uniform = blocks.Uniform(sz);
        if (uniform == default)
            return true;

        if (!uniform.IsSolid)
            return false;

        if (sz == 0 || sz == SectionHeight - 1 ||
            !IsUniformSolid(blocks, sz - 1) ||
            !IsUniformSolid(blocks, sz + 1))
            return true;

        return !IsUniformSolid(cloc + (1, 0), sz) ||
               !IsUniformSolid(cloc + (-1, 0), sz) ||
               !IsUniformSolid(cloc + (0, 1), sz) ||
               !IsUniformSolid(cloc + (0, -1), sz);
    }

    private bool IsUniformSolid(Vec2i cloc, int sz) =>
        blocksRaw.TryGetChunkBlocks(cloc, out var blocks) && IsUniformSolid(blocks, sz);

    private bool IsUniformSolid(ChunkBlocks blocks, int sz)
    {
        var uniform = blocks.Uniform(sz);
        return uniform != default && uniform.IsSolid;
    }

    private bool IsUnavailable(Vec2i cloc) =>
        !chunks.TryGet(cloc, out var chunk) || !chunk.IsLightReady;
}
