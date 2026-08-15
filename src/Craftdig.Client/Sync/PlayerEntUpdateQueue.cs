namespace Craftdig;

public readonly record struct PlayerEntUpdate(
    EntUpdateCommand Command,
    byte[] Buffer,
    int Length);

[Player]
public class PlayerEntUpdateQueue
{
    private readonly ConcurrentQueue<PlayerEntUpdate> updates = [];

    public void Enqueue(EntUpdateCommand command, ReadOnlySpan<byte> data)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(data.Length);
        data.CopyTo(buffer);
        updates.Enqueue(new(command, buffer, data.Length));
    }

    public bool TryDequeue(out PlayerEntUpdate update) => updates.TryDequeue(out update);

    public void Return(PlayerEntUpdate update) => ArrayPool<byte>.Shared.Return(update.Buffer);

    public void Clear()
    {
        while (TryDequeue(out var update))
            Return(update);
    }
}
