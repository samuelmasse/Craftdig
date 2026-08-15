namespace Craftdig;

[Dimension]
public class DimensionSyncScope(WorldSyncScopes scopes)
{
    public uint Id { get; } = scopes.Take();
}
