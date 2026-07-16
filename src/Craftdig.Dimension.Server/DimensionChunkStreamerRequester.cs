namespace Craftdig.Dimension.Server;

[Dimension]
public class DimensionChunkStreamerRequester(
    AppRenderDistance renderDistance,
    DimensionSockets sockets,
    DimensionChunks chunks,
    DimensionChunkStreamer chunkStreamer,
    DimensionEntStreamer entStreamer,
    DimensionPlayerSpawner playerSpawner)
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
        if (!ns.IsWorldSyncReady)
            return false;

        var streamed = ns.SocketStreamedChunks ??= [];
        if (ns.PlayerSpawnPhase == PlayerSpawnPhase.LoadingTerrain)
            return StreamTerrainLoad(ns, streamed);
        if (ns.PlayerSpawnPhase != PlayerSpawnPhase.Active)
            return false;

        var cloc = ns.SocketPlayer.Position.ToLoc().ToSloc().Xy;

        if (!TryGetNearestNotStreamedChunk(cloc, streamed, out var nearest))
            return false;

        if (!chunkStreamer.Stream(ns, nearest))
            return false;

        entStreamer.EnterChunk(ns, nearest);
        streamed.Add(nearest);

        return true;
    }

    private bool StreamTerrainLoad(NetSocket ns, HashSet<Vec2i> streamed)
    {
        var window = new TerrainLoadWindow(ns.TerrainLoadCenter, ns.TerrainLoadSideLength);
        if (!TryGetNextTerrainChunk(window, streamed, out var cloc))
            return false;

        if (!chunkStreamer.Stream(ns, cloc))
            return false;

        entStreamer.EnterChunk(ns, cloc);
        streamed.Add(cloc);

        if (TerrainLoadComplete(window, streamed))
        {
            playerSpawner.Activate(ns);
            return false;
        }

        return true;
    }

    private bool TryGetNextTerrainChunk(
        TerrainLoadWindow window,
        HashSet<Vec2i> streamed,
        out Vec2i cloc)
    {
        cloc = default;
        int nearestDistance = int.MaxValue;

        for (int i = 0; i < window.Count; i++)
        {
            var candidate = window[i];
            if (streamed.Contains(candidate) ||
                !chunks.TryGet(candidate, out var chunk) ||
                !chunk.IsLoaded ||
                !chunk.IsLightReady)
                continue;

            var delta = candidate - window.Center;
            int distance = Math.Abs(delta.X) + Math.Abs(delta.Y);
            if (distance >= nearestDistance)
                continue;

            nearestDistance = distance;
            cloc = candidate;
        }

        return nearestDistance != int.MaxValue;
    }

    private bool TerrainLoadComplete(TerrainLoadWindow window, HashSet<Vec2i> streamed)
    {
        for (int i = 0; i < window.Count; i++)
        {
            if (!streamed.Contains(window[i]))
                return false;
        }

        return true;
    }

    private bool TryGetNearestNotStreamedChunk(Vec2i center, HashSet<Vec2i> streamed, out Vec2i cloc)
    {
        cloc = default;

        for (int r = 0; r <= renderDistance.Far; r++)
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

                bool Visit(Vec2i delta) =>
                    chunks.TryGet(center + delta, out var chunk) &&
                    chunk.IsLoaded &&
                    chunk.IsLightReady &&
                    !streamed.Contains(center + delta);
            }
        }

        return false;
    }
}
