namespace Crafthoe.Dimension;

public readonly record struct ValueChange<T>(Vector3i Loc, T Prev);
