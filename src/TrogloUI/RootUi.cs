namespace TrogloUI;

[Root]
public class RootUi : EntObj
{
    private long nextId = 1;
    private long alive = 1;
    private List<EntObj> ents = [];
    private List<EntObj> buffer = [];

    public RootUi()
    {
        this.UiId = nextId++;
        this.UiRoot = this;
    }

    public EntObj Alloc()
    {
        Console.WriteLine($"ui {nextId}");

        var ent = new EntObj()
        {
            UiId = nextId++,
            UiRoot = this
        };

        ents.Add(ent);

        return ent;
    }

    public void Cleanup()
    {
        alive++;

        Mark(this);

        foreach(var ent in ents)
        {
            if (ent.UiAliveToken == alive)
                buffer.Add(ent);
            else
            {
                Console.WriteLine($"clear {ent.UiId}");
                ent.Clear();
            }
        }

        (ents, buffer) = (buffer, ents);
        buffer.Clear();
    }

    private void Mark(EntObj ent)
    {
        ent.UiAliveToken = alive;

        foreach (var child in ent.Nodes)
            Mark(child);

        foreach (var child in ent.NodeStack)
            Mark(child);
    }
}
