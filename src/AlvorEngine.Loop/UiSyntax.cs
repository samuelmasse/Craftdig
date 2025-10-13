namespace AlvorEngine.Loop;

public static class UiSyntax
{
    public static EntObj Node()
    {
        var val = new EntObj();
        return val;
    }

    public static EntObj Node(out EntObj val)
    {
        val = new();
        return val;
    }

    public static EntObj Node(EntObj parent)
    {
        var val = new EntObj();
        parent.Nodes().Add(val);
        return val;
    }

    public static EntObj Node(EntObj parent, out EntObj val)
    {
        val = new();
        parent.Nodes().Add(val);
        return val;
    }

    public static EntObj Mut(this EntObj ent, Action<EntObj> action)
    {
        action.Invoke(ent);
        return ent;
    }

    public static T? Get<T>(T? value, Func<T>? func) where T : allows ref struct
    {
        if (func != null)
            return func.Invoke();
        else return value;
    }
}
