namespace Crafthoe.Client;

[Player]
public class PlayerWorldIndicesUpdateReceiver(WorldModuleIndices moduleIndices)
{
    public void Receive(ReadOnlySpan<byte> data)
    {
        var csv = Encoding.UTF8.GetString(data);
        var names = csv.Split(',');
        moduleIndices.Apply(names);
    }
}
