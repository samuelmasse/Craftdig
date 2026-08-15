namespace Craftdig;

[Player]
public class PlayerWorldIndicesUpdateReceiver(WorldModuleIndicesMut moduleIndices)
{
    public void Receive(ReadOnlySpan<byte> data)
    {
        var csv = Encoding.UTF8.GetString(data);
        var names = csv.Split(',');
        moduleIndices.Apply(names);
    }
}
