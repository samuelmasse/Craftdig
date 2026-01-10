namespace Craftdig.Client;

[Player]
public class PlayerChunkClientUnloader(DimensionBlocksRaw blocksRaw, PlayerSocket socket)
{
    public void Unload(EntMut ent)
    {
        socket.Send(new ForgetChunkCommand() { Cloc = ent.Cloc() });

        if (blocksRaw.TryGetChunkBlocks(ent.Cloc(), out var blocks))
            blocks.Fill(default);
    }
}
