namespace Craftdig.Client;

[Player]
public class PlayerPongReceiver(AppLog log)
{
    private double latency;

    public double Latency => latency;

    public void Receive(PongCommand cmd)
    {
        var dt = Stopwatch.GetTimestamp() - cmd.Ping.Timestamp;
        latency = dt * 1000 / (double)Stopwatch.Frequency;

        log.Debug("Pong! {0}", latency);
    }
}
