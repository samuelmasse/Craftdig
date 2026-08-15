namespace Craftdig;

[Components]
public interface IWorldComponents
{
    // Universe
    [Saved][Synced] bool IsUniverse { get; set; }
    [Saved][Synced] long Time { get; set; }

    // World location
    [Saved] Vec3d WorldPosition { get; set; }
    [Saved] Ent WorldDimension { get; set; }
    [Saved] bool IsWorldPlayer { get; set; }

    // Common
    [ComponentToString] Guid Id { get; set; }
    bool IsLoaded { get; set; }

    // Dimension scope
    [ComponentToString] bool IsDimensionScope { get; set; }
}
