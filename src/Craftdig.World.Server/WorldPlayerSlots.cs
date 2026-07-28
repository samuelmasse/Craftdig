namespace Craftdig.World.Server;

[World]
public class WorldPlayerSlots(Log log)
{
    private readonly SortedSet<int> set = [];
    private int max = 1;

    public int Take()
    {
        if (set.Count > 0)
        {
            int min = set.Min;
            set.Remove(min);

            log.Debug("Reusing player slot {0}", min);
            return min;
        }
        else
        {
            log.Debug("New player slot {0}", max);
            return max++;
        }
    }

    public void Return(int slot)
    {
        set.Add(slot);
        log.Debug("Returning slot {0}", slot);
    }
}
