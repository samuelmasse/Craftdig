namespace Craftdig;

public static class PresenceChallengeRecords
{
    public static bool AreCanonical(ReadOnlySpan<PresenceChallengeRecord> records)
    {
        for (int i = 1; i < records.Length; i++)
        {
            if (!IsCanonicalPair(records[i - 1], records[i]))
                return false;
        }

        return true;
    }

    public static bool AreCanonical(ReadOnlySpan<byte> recordBytes)
    {
        if (!WireRecords.TryReadAt(recordBytes, 0, out PresenceChallengeRecord previous))
            return false;

        for (int i = 1; i < WireRecords.Count<PresenceChallengeRecord>(recordBytes); i++)
        {
            if (!WireRecords.TryReadAt(recordBytes, i, out PresenceChallengeRecord current) ||
                !IsCanonicalPair(previous, current))
                return false;

            previous = current;
        }

        return true;
    }

    public static bool ContainsSession(ReadOnlySpan<PresenceChallengeRecord> records, SessionId sessionId)
    {
        int low = 0;
        int high = records.Length - 1;
        while (low <= high)
        {
            int middle = low + (high - low) / 2;
            int comparison = records[middle].SessionId.CompareTo(sessionId);
            if (comparison == 0)
                return true;

            if (comparison < 0)
                low = middle + 1;
            else
                high = middle - 1;
        }

        return false;
    }

    private static bool IsCanonicalPair(in PresenceChallengeRecord previous, in PresenceChallengeRecord current) =>
        previous.CompareTo(current) < 0 && previous.SessionId != current.SessionId;
}
