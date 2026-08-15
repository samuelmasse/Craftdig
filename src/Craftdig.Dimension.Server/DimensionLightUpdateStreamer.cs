namespace Craftdig;

[Dimension]
public class DimensionLightUpdateStreamer(
    DimensionLightChanges lightChanges,
    DimensionSockets sockets,
    DimensionLightStreamer lightStreamer)
{
    private readonly Dictionary<Vec2i, uint> masks = [];

    public void Tick()
    {
        foreach (var sloc in lightChanges)
        {
            masks.TryGetValue(sloc.Xy, out uint mask);
            masks[sloc.Xy] = mask | (1u << sloc.Z);
        }

        foreach (var (cloc, mask) in masks)
        {
            if (!lightStreamer.TryEncode(cloc, mask, false, out var command, out var data))
                continue;

            foreach (var socket in sockets.Span)
            {
                var streamed = socket.SocketStreamedChunks;
                if (streamed != null && streamed.Contains(cloc))
                    socket.Send(command, data);
            }
        }

        masks.Clear();
    }
}
