namespace Crafthoe.Dimension;

[Dimension]
public class DimensionPlayers
{
    private readonly List<Entity> players = [];

    public ReadOnlySpan<Entity> Players => CollectionsMarshal.AsSpan(players);

    public void Add(Entity entity) => players.Add(entity);
    public void Remove(Entity entity) => players.Remove(entity);
}
