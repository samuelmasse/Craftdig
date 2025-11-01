namespace Crafthoe.Client;

[Player]
public class PlayerClient(
    DimensionChunks chunks,
    DimensionClientChunkReceiverHandler chunkReceiverHandler,
    PlayerEnt ent,
    PlayerPositionUpdateReceiver positionUpdateReceiver,
    PlayerChunkUpdateQueue chunkUpdateQueue,
    PlayerSocket socket)
{
    public void Tick()
    {
        ent.Ent.PrevPosition() = ent.Ent.Position();
        ent.Ent.Position() = positionUpdateReceiver.Latest.Position;
        ent.Ent.IsFlying() = positionUpdateReceiver.Latest.IsFlying;
        ent.Ent.IsSprinting() = positionUpdateReceiver.Latest.IsSprinting;

        socket.Send(new((int)ServerCommand.MovePlayer,
            MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref ent.Ent.Movement(), 1))));

        ent.Ent.Movement() = default;
    }

    public void Frame()
    {
        int count = chunkUpdateQueue.Count;
        while (count > 0 && chunkUpdateQueue.TryDequeue(out var item))
        {
            var (cloc, blocks) = item;

            chunks.Alloc(cloc);
            var chunk = chunks[cloc];
            chunk.Blocks() = blocks;
            chunkReceiverHandler.Handle(chunk);
        }
    }
}
