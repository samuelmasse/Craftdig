namespace Craftdig;

[Dimension]
public class DimensionChunkArena(DimensionChunkEntIdxContext context) : EntIdxArena(context)
{
    /// <summary>Invalidates chunk Ents before releasing their dimension-owned hooks.</summary>
    public override void Dispose()
    {
        base.Dispose();
        context.Dispose();
    }
}
