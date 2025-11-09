namespace Crafthoe.Client;

[Player]
public class PlayerPositionUpdateReceiver
{
    private readonly PositionUpdateCommand[] latest = new PositionUpdateCommand[16];
    private int index;

    public PositionUpdateCommand Latest => latest[index % latest.Length];

    public void Receive(NetSocket ns, NetMessage msg)
    {
        latest[(index + 1) % latest.Length] = MemoryMarshal.AsRef<PositionUpdateCommand>(msg.Data);
        index++;
    }
}
