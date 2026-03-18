namespace Craftdig.World;

[Components]
public interface IWorldComponents
{
    // World location
    [Saved] Vector3d WorldPosition { get; set; }
    [Saved] Ent WorldDimension { get; set; }
    [Saved] bool IsWorldPlayer { get; set; }

    // Common
    [ComponentToString] Guid Id { get; set; }
    bool IsLoaded { get; set; }

    // Dimension scope
    [ComponentToString] bool IsDimensionScope { get; set; }
}
