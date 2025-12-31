namespace Craftdig.Client;

[Player]
public class PlayerSectionUpdateReceiver(
    WorldModuleIndices moduleIndices,
    PlayerSectionUpdateQueue sectionUpdateQueue)
{
    private readonly EntDecompressor decompressor = new(moduleIndices);
    private readonly ConcurrentBag<Ent[]> pool = [];

    public void Receive(SectionUpdateCommand cmd, ReadOnlySpan<byte> data)
    {
        if (!pool.TryTake(out var section))
            section = new Ent[SectionVolume];

        decompressor.Decompress(data, section);
        sectionUpdateQueue.Enqueue((cmd.Sloc, section));
    }

    public void Return(Ent[] arr) => pool.Add(arr);
}
