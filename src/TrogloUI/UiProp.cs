namespace TrogloUI;

public readonly struct UiProp<T>(T value, Func<T>? func)
{
    public T Resolve() => func != null ? func() : value;

    public static implicit operator UiProp<T>(T value) => new(value, null);
    public static implicit operator UiProp<T>(Func<T> func) => new(default!, func);

    public override string ToString() => func == null ? $"{value}" : func.ToString()!;
}
