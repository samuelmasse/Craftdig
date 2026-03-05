namespace TrogloUI;

public static class UiSyntax
{
    public static EntMutator<EntObj> Node()
    {
        var val = new EntObj();
        return val.Mutate();
    }

    public static EntMutator<EntObj> Node(out EntObj val)
    {
        val = new();
        return val.Mutate();
    }

    public static EntMutator<EntObj> Node(EntObj parent)
    {
        var val = new EntObj();
        parent.Nodes.Add(val);
        return val.Mutate();
    }

    public static EntMutator<EntObj> Node(EntObj parent, out EntObj val)
    {
        val = new();
        parent.Nodes.Add(val);
        return val.Mutate();
    }


    public static T? Get<T>(T? value, Func<T>? func) where T : allows ref struct
    {
        if (func != null)
            return func.Invoke();
        else return value;
    }
}
