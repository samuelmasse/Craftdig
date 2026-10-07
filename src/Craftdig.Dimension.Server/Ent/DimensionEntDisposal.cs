namespace Craftdig;

public readonly record struct DimensionEntDisposal(
    Guid Id,
    Vec2i? Cloc,
    NetSocket? Owner);
