namespace Craftdig;

[Dimension]
public class DimensionChunkBagMut : EntIdxGatedBagMut<DimensionComponents.IsChunk, WorldComponents.IsLoaded>;

[Dimension]
public class DimensionChunkBag(DimensionChunkBagMut bag) :
    EntIdxGatedBag<DimensionComponents.IsChunk, WorldComponents.IsLoaded>(bag);
