namespace Crafthoe.Dimension.Server;

public abstract class DimensionReceiver
{
    public abstract void Receive(NetSocket ns, NetMessage msg);
}
