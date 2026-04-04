namespace TrogloUI;

public struct UiText
{
    public string Value;
    public Func<ReadOnlySpan<char>>? Func;

    public readonly ReadOnlySpan<char> Resolve() => Func != null ? Func() : Value;

    public override readonly string ToString()
    {
        if (Value != null)
            return $"\"{Value}\"";
        else return base.ToString()!;
    }
}
