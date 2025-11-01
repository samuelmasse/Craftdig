namespace Crafthoe.World;

public class NetEcho
{
    public void Receive(NetSocket ns, NetMessage msg)
    {
        Console.WriteLine($"Echo {Encoding.UTF8.GetString(msg.Data)}");
    }

    public Span<byte> Wrap(string text)
    {
        return Encoding.UTF8.GetBytes(text);
    }
}
