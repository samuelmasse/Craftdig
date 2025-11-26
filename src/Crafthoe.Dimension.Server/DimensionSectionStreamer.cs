namespace Crafthoe.Dimension.Server;

[Dimension]
public class DimensionSectionStreamer(
    WorldModuleIndices moduleIndices,
    DimensionBlockChanges blockChanges,
    DimensionSockets sockets,
    DimensionBlocksRaw blocksRaw)
{
    private readonly EntCompressor compressor = new(moduleIndices, SectionVolume);
    private readonly HashSet<Vector3i> slocs = [];

    public void Tick()
    {
        foreach (var c in blockChanges.Span)
            slocs.Add(c.Loc.ToSloc());

        foreach (var sloc in slocs)
        {
            var compressed = compressor.Compress(blocksRaw.Slice(sloc).Span);

            foreach (var ns in sockets.Span)
            {
                var streamedChunks = ns.Ent.SocketStreamedChunks();
                if (streamedChunks == null || !streamedChunks.Contains(sloc.Xy))
                    continue;

                ns.Send(new SectionUpdateCommand() { Sloc = sloc }, compressed);
            }
        }

        slocs.Clear();
    }
}
