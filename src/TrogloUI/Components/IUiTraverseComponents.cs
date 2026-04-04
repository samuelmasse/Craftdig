namespace TrogloUI;

[Components]
public interface IUiTraverseComponents
{
    [ComponentToString] long UiId { get; set; }
    RootUi UiRoot { get; set; }
    long UiAliveToken { get; set; }

    [ComponentLazyInitialize] List<EntMut> Nodes { get; set; }
    [ComponentLazyInitialize] Stack<EntMut> NodeStack { get; set; }

    bool IsDeletedV { get; set; }
    Func<bool>? IsDeletedF { get; set; }

    bool IsDisabledV { get; set; }
    Func<bool>? IsDisabledF { get; set; }

    bool IsOrderedV { get; set; }
    Func<bool>? IsOrderedF { get; set; }

    float OrderValueV { get; set; }
    Func<float>? OrderValueF { get; set; }

    EntMut StackedNodeR { get; set; }
    Memory<EntMut> NodesR { get; set; }
}
