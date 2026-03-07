namespace Craftdig.World;

[Components]
public interface IWorldComponents
{
    // Common
    [ComponentToString] Guid Id { get; set; }
    bool IsLoaded { get; set; }

    // Dimension scope
    [ComponentToString] bool IsDimensionScope { get; set; }

    HashSet<EntPtrIdx> ContextEntLiveSet { get; set; } // TODO: remove this
}
