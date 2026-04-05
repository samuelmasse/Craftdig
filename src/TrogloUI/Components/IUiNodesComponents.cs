namespace TrogloUI.Root;

[Components(SkipBuilder = true)]
public interface IUiNodesComponents
{
    [ComponentLazyInitialize] internal List<EntMut> Nodes { get; set; }
    [ComponentLazyInitialize] internal Stack<EntMut> NodeStack { get; set; }
}
