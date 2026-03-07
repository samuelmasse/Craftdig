namespace Craftdig.Dimension;

[Dimension]
public class DimensionChunkBagMut : EntIdxBagMut<DimensionComponents.IsChunk>;

[Dimension]
public class DimensionChunkBag(DimensionChunkBagMut bag) : EntIdxBag<DimensionComponents.IsChunk>(bag);
