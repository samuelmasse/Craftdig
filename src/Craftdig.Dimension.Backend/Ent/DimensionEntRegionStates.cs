namespace Craftdig;

[Dimension]
public class DimensionEntRegionStates(
    WorldPaths worldPaths,
    DimensionPaths paths,
    WorldEntRegionBuckets buckets,
    DimensionEntRegionReader regionReader) : WorldEntRegionStates(worldPaths, buckets, regionReader)
{
    protected override string Dir(Vec2i rloc) => Path.Join(paths.Ents, $"{rloc.X},{rloc.Y}");
}
