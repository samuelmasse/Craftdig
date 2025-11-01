namespace Crafthoe.Dimension;

[StructLayout(LayoutKind.Sequential)]
public struct MovementStep
{
    public MovementAction Sprint;
    public MovementAction Fly;
    public bool Jump;
    public bool FlyDown;
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
