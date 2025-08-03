namespace Crafthoe.Dimension;

[Dimension]
public class DimensionPlayers
{
    private readonly List<EntRef> players = [];

    public ReadOnlySpan<EntRef> Players => CollectionsMarshal.AsSpan(players);

    public void Add(EntRef entity) => players.Add(entity);
    public void Remove(EntRef entity) => players.Remove(entity);
}
