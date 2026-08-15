namespace Craftdig;

[Components]
public interface IWorldBackendComponents
{
    // Dirty
    EntPloc? Ploc { get; set; }
    bool IsDirty { get; set; }
    bool IsLoading { get; set; }
    ulong[] DirtyComponents { get; set; }
}
