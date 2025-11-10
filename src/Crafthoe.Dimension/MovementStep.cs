namespace Crafthoe.Dimension;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MovementStep
{
    public MovementAction Sprint;
    public MovementAction Fly;
    [MarshalAs(UnmanagedType.I1)]
    public bool Jump;
    [MarshalAs(UnmanagedType.I1)]
    public bool FlyDown;
    [MarshalAs(UnmanagedType.I1)]
    public bool FlyUp;
    public Vector3 Vector;
}

public enum MovementAction : byte
{
    None = 0,
    Start = 1,
    Stop = 2,
    Toggle = 3
}
