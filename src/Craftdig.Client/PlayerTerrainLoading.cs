namespace Craftdig.Client;

[Player]
public class PlayerTerrainLoading(
    PlayerSocket socket,
    PlayerChunks chunks)
{
    private readonly Lock sync = new();
    private BeginTerrainLoadCommand pending;
    private bool hasPending;
    private bool begun;

    public bool Begun => begun;
    public TerrainLoadWindow Window { get; private set; }

    public void Receive(BeginTerrainLoadCommand command)
    {
        lock (sync)
        {
            if (hasPending || begun)
            {
                socket.Disconnect();
                return;
            }

            pending = command;
            hasPending = true;
        }
    }

    public void Frame()
    {
        BeginTerrainLoadCommand command;
        lock (sync)
        {
            if (!hasPending)
                return;

            command = pending;
            hasPending = false;
            Window = new(command.CenterCloc, command.SideLength);
            begun = true;
        }
    }

    public bool IsReady(Vec2i cloc) => chunks.IsReady(cloc);
    public bool IsPending(Vec2i cloc) => chunks.IsPending(cloc);

    public bool Complete
    {
        get
        {
            if (!begun)
                return false;

            for (int i = 0; i < Window.Count; i++)
            {
                if (!chunks.IsReady(Window[i]))
                    return false;
            }

            return true;
        }
    }
}
