namespace Craftdig;

[Player]
public class PlayerChunkClientUnloader(
    DimensionBlocksRaw blocksRaw,
    PlayerEntSync entSync,
    PlayerSocket socket)
{
    public void Unload(EntMutIdx ent)
    {
        entSync.ChunkUnloaded(ent.Cloc);
        socket.Send(new ForgetChunkCommand() { Cloc = ent.Cloc });

        if (blocksRaw.TryGetChunkBlocks(ent.Cloc, out var blocks))
            blocks.Fill(default);
    }
}
