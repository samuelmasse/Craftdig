namespace Craftdig.Server;

[Server]
public class ServerTickTimer(ServerTickCheck tickCheck)
{
    private Timer? timer;

    public void Start()
    {
        timer = new Timer((e) =>
        {
            tickCheck.Signal();
        }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(4));
    }

    public void Stop() => timer?.Dispose();
}
