namespace Crafthoe.Client;

[Player]
public class PlayerPositionUpdateReceiver
{
    private Vector3d latest;

    public Vector3d Latest => latest;

    public void Receive(NetSocket ns, NetMessage msg)
    {
        var position = MemoryMarshal.Cast<byte, Vector3d>(msg.Data)[0];
        latest = position;
    }
}
