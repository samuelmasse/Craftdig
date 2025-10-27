namespace Crafthoe.Dimension;

[Dimension]
public class DimensionContext(
    DimensionScope scope,
    DimensionBlockChanges blockChanges)
{
    public DimensionScope Scope => scope;

    public void Frame()
    {
        blockChanges.Clear();
    }
}
