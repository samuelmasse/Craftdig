namespace Craftdig;

public readonly record struct DimensionInventoryActionRequest(NetSocket Socket, InventoryActionCommand Command);
