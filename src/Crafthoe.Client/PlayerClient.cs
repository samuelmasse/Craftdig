namespace Crafthoe.Client;

[Player]
public class PlayerClient(
    DimensionChunks chunks,
    DimensionChunkBag chunkBag,
    DimensionChunkFrontendReceiver chunkReceiverHandler,
    PlayerEnt ent,
    PlayerPositionUpdateReceiver positionUpdateReceiver,
    PlayerChunkUpdateQueue chunkUpdateQueue,
    PlayerSocket socket)
{
    private readonly List<PositionUpdateCommand> expected = [];
    private int c;

    public void Tick()
    {
        expected.Add(new()
        {
            Position = ent.Ent.Position(),
            Velocity = ent.Ent.Velocity(),
            IsFlying = ent.Ent.IsFlying(),
            IsSprinting = ent.Ent.IsSprinting()
        });

        if (expected.Count > 12)
            expected.RemoveAt(0);

        if (expected.Count >= 12)
        {
            var latest = positionUpdateReceiver.Latest;

            if (!HasMatchingCommand(latest))
            {
                expected.Clear();

                ent.Ent.Position() = latest.Position;
                ent.Ent.Velocity() = latest.Velocity;
                ent.Ent.IsFlying() = latest.IsFlying;
                ent.Ent.IsSprinting() = latest.IsSprinting;

                Console.WriteLine($"Corrected {c++}");
            }
        }
    }

    public void Stream()
    {
        socket.Send(new((int)ServerCommand.MovePlayer,
            MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref ent.Ent.Movement(), 1))));
    }

    private bool HasMatchingCommand(PositionUpdateCommand command)
    {
        foreach (var ex in expected)
        {
            if (Vector3d.DistanceSquared(command.Position, ex.Position) < 1)
                return true;
        }

        return false;
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
            chunkBag.Add(chunk);
            chunkReceiverHandler.Receive(chunk);
        }
    }
}
