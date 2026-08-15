namespace Craftdig;

[Dimension]
public class DimensionSectionUpdateStreamer(
    DimensionBlockChanges blockChanges,
    DimensionSockets sockets,
    DimensionSectionStreamer sectionStreamer,
    DimensionBlocksRaw blocksRaw)
{
    private readonly HashSet<Vec3i> slocs = [];

    public void Tick()
    {
        foreach (var c in blockChanges.Span)
            slocs.Add(c.Loc.ToSloc());

        foreach (var sloc in slocs)
        {
            if (!blocksRaw.TryGetChunkBlocks(sloc.Xy, out var blocks))
                continue;

            var compressed = sectionStreamer.Command(sloc, blocks, out var cmd);

            foreach (var ns in sockets.Span)
            {
                var streamedChunks = ns.SocketStreamedChunks;
                if (streamedChunks == null || !streamedChunks.Contains(sloc.Xy))
                    continue;

                ns.Send(cmd, compressed);
            }
        }

        slocs.Clear();
    }
}
