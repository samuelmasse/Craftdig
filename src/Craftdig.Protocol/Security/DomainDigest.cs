namespace Craftdig;

public static class DomainDigest
{
    public static Hash256 Compute<A, B, C>(ReadOnlySpan<byte> domainSeparator, in A first, in B second, in C third)
        where A : struct, IWireValue<A>
        where B : struct, IWireValue<B>
        where C : struct, IWireValue<C>
    {
        Span<byte> canonical = stackalloc byte[domainSeparator.Length + A.WireSize + B.WireSize + C.WireSize];
        var writer = new SpanWriter(canonical);
        writer.WriteBytes(domainSeparator);
        writer.Write(first);
        writer.Write(second);
        writer.Write(third);
        return Hash256.Compute(canonical);
    }
}
