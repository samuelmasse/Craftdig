namespace Craftdig;

[Dimension]
public class DimensionBackendUnloaderHandlers
{
    private readonly List<Action> handlers = [];

    public void Add(Action handler) => handlers.Add(handler);

    public void Run()
    {
        foreach (var handler in handlers)
            handler();

        handlers.Clear();
    }
}
