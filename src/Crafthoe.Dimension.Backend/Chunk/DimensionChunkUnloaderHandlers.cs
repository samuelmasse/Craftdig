namespace Crafthoe.Dimension.Backend;

[Dimension]
public class DimensionChunkUnloaderHandlers
{
    private readonly List<Action<EntMut>> handlers = [];

    public void Run(EntMut ent)
    {
        foreach (var handler in handlers)
            handler.Invoke(ent);
    }

    public void Add(Action<EntMut> handler) => handlers.Add(handler);
}
