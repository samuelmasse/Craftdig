namespace Crafthoe.Client;

[Player]
public class PlayerChunkClientUnloader(DimensionBlocksPool blocksPool, PlayerSocket socket)
{
    public void Unload(EntMut ent)
    {
        socket.Send((int)ServerCommand.ForgetChunk, new ForgetChunkCommand() { Cloc = ent.Cloc() });

        if (!ent.Blocks().IsEmpty)
            blocksPool.Add(ent.Blocks());
    }
}
