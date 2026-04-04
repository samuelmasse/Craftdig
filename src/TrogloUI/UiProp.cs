namespace TrogloUI;

public readonly record struct UiProp<T>(T Value, Func<T>? Func)
{
    public T Resolve() => Func != null ? Func() : Value;

    public static implicit operator UiProp<T>(T value) => new(value, null);
    public static implicit operator UiProp<T>(Func<T> func) => new(default!, func);
}
