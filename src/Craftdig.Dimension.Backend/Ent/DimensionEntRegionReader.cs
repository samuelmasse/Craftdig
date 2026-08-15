namespace Craftdig;

[Dimension]
public class DimensionEntRegionReader(
    DimensionEntArena entArena,
    WorldComponentWriters componentWriters,
    WorldEntRegionBuckets entRegionBuckets,
    DimensionEntRegionFileHandles entRegionFileHandles) :
    WorldEntRegionReader(entArena, componentWriters, entRegionBuckets, entRegionFileHandles);
