namespace TrogloUI;

[Root]
public class RootUi : EntObj
{
    public static implicit operator EntMut(RootUi value) => (EntMut)(value as EntObj);

    private readonly EntArena arena = new();
    private long nextId = 1;
    private long alive = 1;
    private List<EntPtr> ents = [];
    private List<EntPtr> buffer = [];

    public RootUi()
    {
        this.UiId = nextId++;
        this.UiRoot = this;
    }

    ~RootUi() => arena.Dispose();

    public EntMut Alloc()
    {
        var ent = arena.Alloc();
        ent.UiId = nextId++;
        ent.UiRoot = this;

        ents.Add(ent);

        return ent;
    }

    public void Cleanup()
    {
        alive++;

        Mark((EntMut)this);

        foreach (var ent in ents)
        {
            if (ent.UiAliveToken == alive)
                buffer.Add(ent);
            else ent.Dispose();
        }

        (ents, buffer) = (buffer, ents);
        buffer.Clear();
    }

    private void Mark(EntMut ent)
    {
        ent.UiAliveToken = alive;

        foreach (var child in ent.Nodes)
            Mark(child);

        foreach (var child in ent.NodeStack)
            Mark(child);
    }
}
