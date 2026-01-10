namespace Craftdig.Dimension;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ConstructionStep
{
    public int Arg;
    public ConstructionAction Action;
}

public enum ConstructionAction : byte
{
    None,
    Place,
    Remove
}
