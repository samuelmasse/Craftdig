namespace Craftdig.Dimension.Server;

[Dimension]
public class DimensionChunkStreamerRequester(
    DimensionDrawDistance drawDistance,
    DimensionSockets sockets,
    DimensionChunks chunks,
    DimensionChunkBag chunkBag,
    DimensionChunkStreamer chunkStreamer)
{
    private readonly Stopwatch watch = new();
    private readonly List<bool> next = [];

    public void Tick()
    {
        if (sockets.Span.IsEmpty)
            return;

        var sockSpan = sockets.Span;
        for (int i = 0; i < sockSpan.Length; i++)
        {
            if (next.Count <= i)
                next.Add(true);
            else next[i] = true;
        }

        watch.Restart();
        bool anyNext = true;

        while (anyNext && !TimeExceeded())
        {
            anyNext = false;

            for (int i = 0; i < sockSpan.Length && !TimeExceeded(); i++)
            {
                if (!next[i])
                    continue;

                anyNext = true;
                next[i] = StreamNearestChunk(sockSpan[i]);
            }
        }

        bool TimeExceeded() => watch.Elapsed.TotalMilliseconds >= 1;
    }

    private bool StreamNearestChunk(NetSocket ns)
    {
        var cloc = ns.Ent.SocketPlayer().Position().ToLoc().ToSloc().Xy;
        ref var streamed = ref ns.Ent.SocketStreamedChunks();
        streamed ??= [];

        if (!TryGetNearestNotStreamedChunk(cloc, streamed, out var nearest))
            return false;

        chunkStreamer.Stream(ns, nearest);
        streamed.Add(nearest);

        return true;
    }

    private bool TryGetNearestNotStreamedChunk(Vector2i center, HashSet<Vector2i> streamed, out Vector2i cloc)
    {
        cloc = default;

        for (int r = 0; r <= drawDistance.Far; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                int dy = r - Math.Abs(dx);

                if (Visit((dx, dy)))
                {
                    cloc = center + (dx, dy);
                    return true;
                }

                if (Visit((dx, -dy)))
                {
                    cloc = center + (dx, -dy);
                    return true;
                }

                bool Visit(Vector2i delta) =>
                    chunks.TryGet(center + delta, out var chunk) &&
                    chunkBag.Contains(chunk) &&
                    !streamed.Contains(center + delta);
            }
        }

        return false;
    }
}
