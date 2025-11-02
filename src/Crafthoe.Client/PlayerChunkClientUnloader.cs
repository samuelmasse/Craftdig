namespace Crafthoe.Client;

[Player]
public class PlayerChunkClientUnloader(DimensionBlocksPool blocksPool, PlayerSocket socket)
{
    public void Unload(EntMut ent)
    {
        socket.Send(new((int)ServerCommand.ForgetChunk,
            MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref ent.Cloc(), 1))));

        if (!ent.Blocks().IsEmpty)
            blocksPool.Add(ent.Blocks());
    }
}
