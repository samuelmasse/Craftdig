namespace Crafthoe.App;

public class NetEcho
{
    public const int Type = 1;

    public void Receive(NetSocket ns, NetMessage msg)
    {
        Console.WriteLine($"Echo {Encoding.UTF8.GetString(msg.Data)}");
    }

    public NetMessage Wrap(string text)
    {
        return new(Type, Encoding.UTF8.GetBytes(text));
    }
}
