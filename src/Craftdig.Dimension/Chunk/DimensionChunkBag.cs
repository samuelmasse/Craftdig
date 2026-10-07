namespace Craftdig;

[Dimension]
public class DimensionChunkBag :
    EntIdxGatedBag<DimensionComponents.IsChunk, WorldComponents.IsLoaded>;
