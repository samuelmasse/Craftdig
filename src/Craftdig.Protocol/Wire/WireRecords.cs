namespace Craftdig.Protocol;

public static class WireRecords
{
    public static int Count<T>(ReadOnlySpan<byte> recordBytes) where T : struct, IWireValue<T> =>
        recordBytes.Length / T.WireSize;

    public static bool HasShape<T>(ReadOnlySpan<byte> recordBytes, int minCount, int maxCount) where T : struct, IWireValue<T> =>
        recordBytes.Length % T.WireSize == 0 &&
        Count<T>(recordBytes) >= minCount &&
        Count<T>(recordBytes) <= maxCount;

    public static bool TryReadAt<T>(ReadOnlySpan<byte> recordBytes, int index, out T record) where T : struct, IWireValue<T>
    {
        if ((uint)index >= (uint)Count<T>(recordBytes))
        {
            record = default;
            return false;
        }

        return T.TryRead(recordBytes.Slice(index * T.WireSize, T.WireSize), out record);
    }

    public static void WriteAll<T>(ref SpanWriter writer, ReadOnlySpan<T> records) where T : struct, IWireValue<T>
    {
        foreach (ref readonly var record in records)
            writer.Write(record);
    }
}
