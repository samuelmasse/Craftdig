namespace TrogloUI;

public readonly struct UiCallback<T>(T value)
{
    public T Resolve() => value;

    public static implicit operator UiCallback<T>(T value) => new(value);

    public override string ToString() => $"{value}";
}
