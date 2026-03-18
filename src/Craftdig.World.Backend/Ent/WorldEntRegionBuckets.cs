namespace Craftdig.World.Backend;

public class WorldEntRegionBuckets
{
    private readonly int[] sizes;

    public int Count => sizes.Length;
    public ReadOnlySpan<int> Sizes => sizes;

    public WorldEntRegionBuckets()
    {
        sizes = new int[16];

        int size = 1;
        for (int i = 1; i < sizes.Length; i++)
        {
            sizes[i] = size;
            size *= 2;
        }
    }

    public int BestFit(int bytes) => bytes == 0 ? 0 :
        33 - System.Numerics.BitOperations.LeadingZeroCount((uint)bytes - 1);
}
