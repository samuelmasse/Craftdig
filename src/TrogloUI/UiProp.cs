namespace TrogloUI;

public struct UiProp<T>
{
    public T Value;
    public Func<T>? Func;

    public readonly T Resolve() => Func != null ? Func() : Value;
}
