namespace Craftdig.Dimension.Backend;

public class RegionFiles
{
    private readonly string root;
    private readonly string index;
    private readonly string[] buckets;

    public string Index => index;
    public ReadOnlySpan<string> Buckets => buckets;

    public RegionFiles(string dir, Vector2i rloc)
    {
        root = Path.Join(dir, $"{rloc.X},{rloc.Y}");
        index = Path.Join(root, "Index.crhi");
        buckets = new string[16];

        for (int i = 0; i < buckets.Length; i++)
            buckets[i] = Path.Join(root, $"Bucket{i}.crhb");
    }
}
