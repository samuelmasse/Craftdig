namespace Craftdig.Dimension.Server;

public readonly record struct DimensionInventoryActionRequest(NetSocket Socket, InventoryActionCommand Command);
