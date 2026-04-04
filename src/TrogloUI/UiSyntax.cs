namespace TrogloUI;

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

    public static EntMutator<EntMut> NodeStack(EntMut parent)
    {
        var val = parent.UiRoot.Alloc();
        parent.NodeStack.Push(val);
        return val.Mutate();
    }

    public static EntMutator<EntMut> NodeStack(EntMut parent, out EntMut val)
    {
        val = parent.UiRoot.Alloc();
        parent.NodeStack.Push(val);
        return val.Mutate();
    }

    public static T? Get<T>(T? value, Func<T>? func) where T : allows ref struct
    {
        if (func != null)
            return func.Invoke();
        else return value;
    }
}
