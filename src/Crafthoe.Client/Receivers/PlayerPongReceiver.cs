namespace Crafthoe.Client;

[Player]
public class PlayerPongReceiver
{
    public void Receive(NetSocket ns, NetMessage msg)
    {
        var pong = MemoryMarshal.AsRef<PongCommand>(msg.Data);

        var dt = Stopwatch.GetTimestamp() - pong.Ping.Timestamp;
        Console.WriteLine(dt);
        var ms = dt * 1000 / (double)Stopwatch.Frequency;

        Console.WriteLine($"Pong! {ms}");
    }
}
