namespace Craftdig;

internal sealed class PlayerPresenceSigningGuard(int capacity)
{
    private readonly Dictionary<Hash256, long> deadlines = [];

    public int Count => deadlines.Count;
    public bool HasCapacity => deadlines.Count < capacity;

    public void Expire(long monotonicNow)
    {
        Span<Hash256> expired = stackalloc Hash256[capacity];
        int count = 0;
        foreach (var entry in deadlines)
        {
            if (monotonicNow > entry.Value)
                expired[count++] = entry.Key;
        }

        for (int i = 0; i < count; i++)
            deadlines.Remove(expired[i]);
    }

    public bool Contains(Hash256 roundHash) => deadlines.ContainsKey(roundHash);

    public bool TryRecord(Hash256 roundHash, long deadline)
    {
        if (!HasCapacity || deadlines.ContainsKey(roundHash))
            return false;

        deadlines.Add(roundHash, deadline);
        return true;
    }

    public void Clear() => deadlines.Clear();
}
