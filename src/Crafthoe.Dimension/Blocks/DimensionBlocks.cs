namespace Craftdig.Dimension;

[Dimension]
public class DimensionBlocks(DimensionBlocksRaw blocksRaw, DimensionBlockChanges blockChanges)
{
    public bool TryGet(Vector3i loc, out Ent block)
    {
        return blocksRaw.TryGet(loc, out block);
    }

    public bool TrySet(Vector3i loc, Ent block)
    {
        if (!blocksRaw.TryGet(loc, out var prev) || !blocksRaw.TrySet(loc, block))
            return false;

        if (block != prev)
            blockChanges.Add(loc, prev);

        return true;
    }
}
