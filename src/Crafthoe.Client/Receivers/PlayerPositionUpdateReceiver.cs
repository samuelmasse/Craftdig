namespace Crafthoe.Client;

[Player]
public class PlayerPositionUpdateReceiver
{
    private PositionUpdateCommand latest;

    public PositionUpdateCommand Latest => latest;

    public void Receive(NetSocket ns, NetMessage msg) =>
        latest = MemoryMarshal.AsRef<PositionUpdateCommand>(msg.Data);
}
