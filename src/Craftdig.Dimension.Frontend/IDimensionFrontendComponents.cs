namespace Craftdig.Dimension.Frontend;

[Components]
public interface IDimensionFrontendComponents
{
    // Remote rigid presentation
    RemotePositionInterpolation RemotePosition { get; set; }
    RemoteLookAtInterpolation RemoteLookAt { get; set; }

    // Chunk
    Memory<EntPtrIdx> Sections { get; set; }
    SortedList<int, int> Unrendered { get; set; }
    bool IsUnrenderedListBuilt { get; set; }
    bool IsReadyToRender { get; set; }
    SortedList<int, int> Rendered { get; set; }

    // Section
    [ComponentToString] bool IsSection { get; set; }
    [ComponentToString] Vec3i Sloc { get; set; }
    int MeshRevision { get; set; }
    bool IsMeshPending { get; set; }
    bool IsMeshDirty { get; set; }
    SectionMesh TerrainMesh { get; set; }
    EntMutIdx Chunk { get; set; }
}
