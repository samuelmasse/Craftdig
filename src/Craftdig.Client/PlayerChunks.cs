namespace Craftdig.Client;

[Player]
public class PlayerChunks(
    DimensionChunks chunks,
    DimensionChunkFrontendReceiver chunkReceiverHandler,
    DimensionLightChanges lightChanges,
    PlayerChunkUpdateQueue chunkUpdateQueue,
    PlayerChunkLightUpdateQueue lightUpdateQueue,
    PlayerEntSync entSync)
{
    private readonly int maxChunkCompletionsPerFrame = 2;
    private readonly Dictionary<Vec2i, ChunkBlocks> pendingBlocks = [];
    private readonly Dictionary<Vec2i, PlayerChunkLightUpdate> pendingLights = [];
    private readonly Stopwatch watch = new();

    public bool IsReady(Vec2i cloc) =>
        chunks.TryGet(cloc, out var chunk) &&
        chunk.IsLoaded &&
        chunk.IsLightReady;

    public bool IsPending(Vec2i cloc) =>
        pendingBlocks.ContainsKey(cloc) || pendingLights.ContainsKey(cloc);

    public void Frame()
    {
        int blockCount = chunkUpdateQueue.Count;
        int lightCount = lightUpdateQueue.Count;
        int completed = 0;
        watch.Restart();

        while ((blockCount > 0 || lightCount > 0) &&
               completed < maxChunkCompletionsPerFrame &&
               watch.Elapsed.TotalMilliseconds < 2)
        {
            bool progressed = false;
            if (blockCount > 0 && chunkUpdateQueue.TryDequeue(out var item))
            {
                var (cloc, blocks) = item;
                pendingBlocks[cloc] = blocks;
                if (TryComplete(cloc))
                    completed++;

                blockCount--;
                progressed = true;
            }

            if (completed < maxChunkCompletionsPerFrame &&
                lightCount > 0 &&
                lightUpdateQueue.TryDequeue(out var update))
            {
                if (chunks.TryGet(update.Cloc, out var chunk) && chunk.IsLoaded)
                    Apply(chunk, update);
                else
                {
                    if (pendingLights.Remove(update.Cloc, out var previous))
                        previous.Light.Clear();

                    pendingLights.Add(update.Cloc, update);
                    if (TryComplete(update.Cloc))
                        completed++;
                }

                lightCount--;
                progressed = true;
            }

            if (!progressed)
                break;
        }
    }

    private bool TryComplete(Vec2i cloc)
    {
        if (!pendingBlocks.Remove(cloc, out var blocks) || !pendingLights.Remove(cloc, out var update))
        {
            if (blocks != null)
                pendingBlocks.Add(cloc, blocks);
            return false;
        }

        chunks.Alloc(cloc);
        var chunk = chunks[cloc];
        chunk.ChunkBlocks = blocks;
        chunk.ChunkLight = update.Light;
        chunk.IsLoaded = true;
        chunk.IsLightReady = true;
        chunkReceiverHandler.Receive(chunk);
        entSync.ChunkLoaded(cloc);
        return true;
    }

    private void Apply(EntMutIdx chunk, PlayerChunkLightUpdate update)
    {
        var current = chunk.ChunkLight;
        if (update.Full || current == null)
        {
            current?.Clear();
            chunk.ChunkLight = update.Light;
        }
        else
        {
            current.CopyFrom(update.Light, update.SectionMask);
            update.Light.Clear();
        }

        for (int sz = 0; sz < SectionHeight; sz++)
        {
            if ((update.SectionMask & (1u << sz)) != 0)
                lightChanges.AddSection(new(chunk.Cloc, sz));
        }
    }
}
