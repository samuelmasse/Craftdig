namespace Craftdig.Dimension.Server;

public abstract class DimensionReceiver<C>
{
    public abstract void Receive(NetSocket ns, C cmd);
}
