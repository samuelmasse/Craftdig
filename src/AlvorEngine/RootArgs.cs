namespace AlvorEngine;

[Root]
public record class RootArgs
{
    public required IWindow Window { get; init; }
    public required Gld Driver { get; init; }
    public required Type BootState { get; init; }
}
