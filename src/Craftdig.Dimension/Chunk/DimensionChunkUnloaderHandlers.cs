namespace Craftdig.Dimension;

[Dimension]
public class DimensionChunkUnloaderHandlers
{
    private readonly List<Action<EntMutIdx>> handlers = [];

    public void Run(EntMutIdx ent)
    {
        foreach (var handler in handlers)
            handler.Invoke(ent);
    }

    public void Add(Action<EntMutIdx> handler) => handlers.Add(handler);
}
