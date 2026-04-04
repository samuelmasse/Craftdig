namespace TrogloUI;

public struct UiCallback<T>
{
    public T Value;

    public readonly T Resolve() => Value;
}
