namespace Crafthoe.Client;

[Player]
public class PlayerPositionUpdateReceiver
{
    private readonly PositionUpdateCommand[] latest = new PositionUpdateCommand[64];
    private int index;

    public PositionUpdateCommand Latest => latest[index % latest.Length];

    public void Receive(PositionUpdateCommand cmd)
    {
        latest[(index + 1) % latest.Length] = cmd;
        index++;
    }
}
