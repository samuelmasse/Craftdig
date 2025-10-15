namespace Crafthoe.Dimension;

[Dimension]
public class DimensionRegionBuckets
{
    private readonly int[] sizes;
    private readonly int unit;

    public int Count => sizes.Length;
    public ReadOnlySpan<int> Sizes => sizes;
    public int Unit => unit;

    public DimensionRegionBuckets()
    {
        unit = RegionBlockEntry.Size;

        sizes = new int[16];

        int size = 1;
        for (int i = 1; i < sizes.Length; i++)
        {
            sizes[i] = size * unit;
            size *= 2;
        }
    }

    public int BestFit(int bytes) => bytes == 0 ? 0 :
        33 - System.Numerics.BitOperations.LeadingZeroCount((uint)Math.Ceiling(bytes / (float)unit) - 1);
}
