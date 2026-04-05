namespace TrogloUI;

using TrogloUI.Root;

public static class UiSyntax
{
    public static EntMutator<EntMut> Node(EntMut parent)
    {
        var val = parent.UiRoot.Alloc();
        parent.Nodes.Add(val);
        return val.Mutate();
    }

    public static EntMutator<EntMut> Node(EntMut parent, out EntMut val)
    {
        val = parent.UiRoot.Alloc();
        parent.Nodes.Add(val);
        return val.Mutate();
    }

    public static EntMutator<EntMut> NodeS(EntMut parent)
    {
        var val = parent.UiRoot.Alloc();
        parent.NodeStack.Push(val);
        return val.Mutate();
    }

    public static Span<EntMut> Nodes(EntMut parent) => CollectionsMarshal.AsSpan(parent.Nodes);

    public static void NodesClear(EntMut parent) => parent.Nodes.Clear();

    public static int NodesCount(EntMut parent) => parent.Nodes.Count;

    public static void NodesAdd(EntMut parent, EntMut child) => parent.Nodes.Add(child);

    public static bool NodesRemove(EntMut parent, EntMut child) => parent.Nodes.Remove(child);

    public static void NodesRemoveAt(EntMut parent, int index) => parent.Nodes.RemoveAt(index);

    public static Stack<EntMut> NodeStackSpan(EntMut parent) => parent.NodeStack;

    public static int NodeStackCount(EntMut parent) => parent.NodeStack.Count;

    public static EntMut NodeStackPop(EntMut parent) => parent.NodeStack.Pop();

    public static bool NodeStackTryPeek(EntMut parent, out EntMut child) => parent.NodeStack.TryPeek(out child);
}
