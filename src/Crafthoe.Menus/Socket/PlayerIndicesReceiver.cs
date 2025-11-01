namespace Crafthoe.Menus;

[Player]
public class PlayerIndicesReceiver(WorldModuleIndices moduleIndices)
{
    public void Receive(NetSocket ns, NetMessage msg)
    {
        var csv = Encoding.UTF8.GetString(msg.Data);
        var names = csv.Split(',');
        moduleIndices.Apply(names);
    }
}
