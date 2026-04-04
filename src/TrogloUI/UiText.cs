namespace TrogloUI;

public readonly record struct UiText(string Value, Func<ReadOnlySpan<char>>? Func)
{
    public ReadOnlySpan<char> Resolve() => Func != null ? Func() : Value;

    public static implicit operator UiText(string value) => new(value, null);
    public static implicit operator UiText(Func<ReadOnlySpan<char>> func) => new(null!, func);

    public override string ToString()
    {
        if (Value != null)
            return $"\"{Value}\"";
        else return base.ToString()!;
    }
}
