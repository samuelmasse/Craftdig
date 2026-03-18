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
    protected override Vector2i Rloc(EntMutIdx ent) => ent.Position.ToLoc().Xy.ToCloc().ToRloc();
}
