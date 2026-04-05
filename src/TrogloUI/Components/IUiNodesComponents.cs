namespace TrogloUI.Root;

[Components]
internal interface IUiNodesComponents
{
    [ComponentToString] internal long UiId { get; set; }
    internal RootUi UiRoot { get; set; }
    internal long UiAliveToken { get; set; }

    [ComponentLazyInitialize] internal List<EntMut> Nodes { get; set; }
    [ComponentLazyInitialize] internal Stack<EntMut> NodeStack { get; set; }
}
