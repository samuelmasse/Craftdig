namespace Crafthoe.Dimension.Server;

[Dimension]
public class DimensionSectionReminder(
    DimensionSockets sockets,
    DimensionSectionStreamer sectionStreamer,
    DimensionBlocksRaw blocksRaw)
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
                next[i] = StreamForgottenSection(sockSpan[i]);
            }
        }

        bool TimeExceeded() => watch.Elapsed.TotalMilliseconds >= 1;
    }

    private bool StreamForgottenSection(NetSocket ns)
    {
        var sections = ns.Ent.SocketForgottenSections();
        var queue = ns.Ent.SocketForgottenSectionQueue();

        if (sections == null || queue == null)
            return false;

        if (queue.Count == 0)
            return false;

        var sloc = queue.Dequeue();
        sections.Remove(sloc);

        if (!blocksRaw.TryGetChunkBlocks(sloc.Xy, out var blocks))
            return false;

        var compressed = sectionStreamer.Command(sloc, blocks, out var cmd);
        ns.Send(cmd, compressed);

        return true;
    }
}
