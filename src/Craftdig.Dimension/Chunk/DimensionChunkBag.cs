namespace Craftdig.Dimension;

[Dimension]
public class DimensionChunkBagMut : EntIdxBagMut<DimensionComponents.IsChunkLoaded>;

[Dimension]
public class DimensionChunkBag(DimensionChunkBagMut bag) : EntIdxBag<DimensionComponents.IsChunkLoaded>(bag);
