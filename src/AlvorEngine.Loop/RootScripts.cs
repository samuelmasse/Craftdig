namespace AlvorEngine.Loop;

public class RootScripts
{
    private readonly List<Script> scripts = [];

    public ReadOnlySpan<Script> Span => CollectionsMarshal.AsSpan(scripts);

    public void Add(Script script)
    {
        scripts.Add(script);
        scripts.Sort((a, b) => a.Order.CompareTo(b.Order));
        script.Load();
    }

    public void Remove(Script script)
    {
        scripts.Remove(script);
        script.Unload();
    }
}
