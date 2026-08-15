namespace Craftdig;

public class EntRegionFiles
{
    private readonly string root;
    private readonly string[] buckets;

    public ReadOnlySpan<string> Buckets => buckets;

    public EntRegionFiles(string dir)
    {
        root = dir;
        buckets = new string[16];

        for (int i = 0; i < buckets.Length; i++)
            buckets[i] = Path.Join(root, $"Bucket{i}.crhb");
    }
}
