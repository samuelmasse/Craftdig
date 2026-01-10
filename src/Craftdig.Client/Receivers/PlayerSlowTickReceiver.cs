namespace Craftdig.Client;

[Player]
public class PlayerSlowTickReceiver
{
    private int count;
    private int register;

    public void Receive()
    {
        count++;
    }

    public bool ShouldSlowTick()
    {
        bool val = count > register;
        register = count;
        return val;
    }
}
