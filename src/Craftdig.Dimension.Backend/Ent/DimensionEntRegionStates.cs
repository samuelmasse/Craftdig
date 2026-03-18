namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionEntRegionStates(
    WorldPaths worldPaths,
    DimensionPaths paths,
    WorldEntRegionBuckets buckets,
    DimensionEntRegionReader regionReader) : WorldEntRegionStates(worldPaths, buckets, regionReader)
{
    protected override string Dir(Vector2i rloc) => Path.Join(paths.Ents, $"{rloc.X},{rloc.Y}");
}
