namespace TrogloUI;

[Root]
public class RootUiUpdate
{
    internal void Update(EntMut n)
    {
        if (n.HasOnUpdateF)
            n.OnUpdateFDelegate?.Invoke();

        foreach (var c in n.NodesR.Span)
            Update(c);
    }
}
