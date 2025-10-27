namespace Crafthoe.Server;

[World]
public class WorldServerTickTimer(WorldServerTickCheck tickCheckQueue)
{
    private Timer? timer;

    public void Start()
    {
        timer = new Timer((e) =>
        {
            tickCheckQueue.Signal();
        }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(20));
    }
}
