namespace Craftdig.Dimension.Backend;

[Components]
public interface IDimensionBackendComponents
{
    Vector2i? RigidRloc { get; set; }
    long RigidPersistId { get; set; }
}
