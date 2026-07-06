namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionEntRegionWriter(
    DimensionIndexedComponents indexedComponents,
    WorldComponentWriters componentWriters,
    WorldEntRegionBuckets entRegionBuckets,
    DimensionEntRegionFileHandles entRegionFileHandles,
    DimensionEntRegionStates entRegionStates) :
    WorldEntRegionWriter(indexedComponents, componentWriters, entRegionBuckets, entRegionFileHandles, entRegionStates)
{
    protected override Vec2i Rloc(EntMutIdx ent) => ent.Position.ToLoc().XY.ToCloc().ToRloc();
}
