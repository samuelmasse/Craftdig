namespace Crafthoe.Client;

[Player]
public class PlayerPongReceiver(AppLog log)
{
    public void Receive(PongCommand cmd)
    {
        var dt = Stopwatch.GetTimestamp() - cmd.Ping.Timestamp;
        var ms = dt * 1000 / (double)Stopwatch.Frequency;

        log.Debug("Pong! {0}", ms);
    }
}
