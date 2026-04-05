namespace TrogloUI;

public readonly struct UiText(string value, Func<ReadOnlySpan<char>>? func)
{
    public ReadOnlySpan<char> Resolve() => func != null ? func() : value;

    public static implicit operator UiText(string value) => new(value, null);
    public static implicit operator UiText(Func<ReadOnlySpan<char>> func) => new(null!, func);

    public override string ToString()
    {
        if (value != null)
            return $"\"{value}\"";
        else return base.ToString()!;
    }
}
