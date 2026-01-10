namespace TrogloUI;

[Root]
public class RootUiUpdate
{
    internal void Update(EntObj n)
    {
        if (n.HasOnUpdateF())
            n.OnUpdateF()?.Invoke();

        foreach (var c in n.GetNodesR().Span)
            Update(c);
    }
}
