namespace Craftdig;

public readonly record struct ValueChange<T>(Vec3i Loc, T Prev);
