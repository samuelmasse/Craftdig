namespace Craftdig;

public readonly record struct DimensionEntDisposal(
    EntMutIdx Ent,
    Guid Id,
    Vec2i? Cloc,
    NetSocket? Owner);
