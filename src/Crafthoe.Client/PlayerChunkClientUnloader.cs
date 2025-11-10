namespace Crafthoe.Client;

[Player]
public class PlayerChunkClientUnloader(DimensionBlocksPool blocksPool, PlayerSocket socket)
{
    public void Unload(EntMut ent)
    {
        socket.Send(new ForgetChunkCommand() { Cloc = ent.Cloc() });

        if (!ent.Blocks().IsEmpty)
            blocksPool.Add(ent.Blocks());
    }
}
