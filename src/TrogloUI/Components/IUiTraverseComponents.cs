namespace TrogloUI;

[Components]
public interface IUiTraverseComponents
{
    [ComponentToString] long UiId { get; set; }
    RootUi UiRoot { get; set; }
    long UiAliveToken { get; set; }

    [ComponentLazyInitialize] List<EntObj> Nodes { get; set; }
    [ComponentLazyInitialize] Stack<EntObj> NodeStack { get; set; }

    bool IsDeletedV { get; set; }
    Func<bool>? IsDeletedF { get; set; }

    bool IsDisabledV { get; set; }
    Func<bool>? IsDisabledF { get; set; }

    bool IsOrderedV { get; set; }
    Func<bool>? IsOrderedF { get; set; }

    float OrderValueV { get; set; }
    Func<float>? OrderValueF { get; set; }

    EntObj? StackedNodeR { get; set; }
    Memory<EntObj> NodesR { get; set; }
}
