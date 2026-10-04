namespace Craftdig;

[Dimension]
public class DimensionChunkArena(DimensionChunkEntIdxContextBuilder context) : EntIdxArena(context.Ent)
{
    /// <summary>Invalidates chunk Ents before releasing their dimension-owned hooks.</summary>
    public override void Dispose()
    {
        base.Dispose();
        context.Dispose();
    }
}
