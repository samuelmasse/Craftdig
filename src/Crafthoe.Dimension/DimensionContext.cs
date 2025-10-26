namespace Crafthoe.Dimension;

[Dimension]
public class DimensionContext(
    DimensionScope scope,
    DimensionBlockChanges blockChanges,
    DimensionRigids rigids)
{
    public DimensionScope Scope => scope;

    public void Tick()
    {
        rigids.Tick();
    }

    public void Frame()
    {
        blockChanges.Clear();
    }
}
