namespace Craftdig.Dimension;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct DropStep
{
    public int Arg;
    public DropAction Action;
}

public enum DropAction : byte
{
    None,
    DropTest
}
