namespace Craftdig.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct EntSyncSchemaCommand(ulong worldSchema, ulong dimensionSchema) : ICommand
{
    public readonly ulong WorldSchema = worldSchema;
    public readonly ulong DimensionSchema = dimensionSchema;

    public static ushort CommandId => (ushort)Commands.EntSyncSchema;
}
