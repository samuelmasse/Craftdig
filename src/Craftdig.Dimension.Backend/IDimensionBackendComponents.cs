namespace Craftdig.Dimension.Backend;

[Components]
public interface IDimensionBackendComponents
{
    // Rigid
    Vector2i? RigidRloc { get; set; }
    long RigidPersistId { get; set; }
}
