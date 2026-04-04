namespace TrogloUI;

public readonly record struct UiCallback<T>(T Value)
{
    public T Resolve() => Value;

    public static implicit operator UiCallback<T>(T value) => new(value);
}
