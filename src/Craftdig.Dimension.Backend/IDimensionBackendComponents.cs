namespace Craftdig.Dimension.Backend;

[Components]
public interface IDimensionBackendComponents
{
    // Dirty
    EntPloc? Ploc { get; set; }
    bool IsDirty { get; set; }
    ulong[] DirtyComponents { get; set; }

    // Chunk
    bool IsChunkComponentsLoaded { get; set; }
}
