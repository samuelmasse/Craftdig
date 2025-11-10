namespace Crafthoe.Client;

[Player]
public class PlayerPongReceiver
{
    public void Receive(PongCommand cmd)
    {
        var dt = Stopwatch.GetTimestamp() - cmd.Ping.Timestamp;
        var ms = dt * 1000 / (double)Stopwatch.Frequency;

        Console.WriteLine($"Pong! {ms}");
    }
}
