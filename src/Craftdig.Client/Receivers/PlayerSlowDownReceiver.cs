namespace Craftdig.Client;

[Player]
public class PlayerSlowDownReceiver
{
    private int count;
    private int register;

    public void Receive()
    {
        count++;
    }

    public bool ShouldSlowDown()
    {
        bool val = count > register;
        register = count;
        return val;
    }
}
