namespace Craftdig.World.Backend;

[World]
public class WorldComponentWriters(WorldComponentIndices indices, WorldScope scope)
{
    private WorldComponentWriter[] writers = [];

    public WorldComponentWriter this[int index]
    {
        get
        {
            if (index >= writers.Length)
                Array.Resize(ref writers, MathHelper.NextPowerOfTwo(index + 1));

            ref var writer = ref writers[index];
            writer ??= New(indices[index]);

            return writer;
        }
    }

    private WorldComponentWriter New(EntComponent component)
    {
        Type? type;

        if (component.ValueType.IsArray)
            type = typeof(WorldComponentArrayWriter<,>).MakeGenericType(component.ValueType.GetElementType()!, component.NameType);
        else
        {
            if (component.ValueType == typeof(Ent))
                type = typeof(WorldComponentEntWriter<>).MakeGenericType(component.NameType);
            else if (component.ValueType == typeof(EntMutIdx))
                type = typeof(WorldComponentEntMutIdxWriter<>).MakeGenericType(component.NameType);
            else type = typeof(WorldComponentWriter<,>).MakeGenericType(component.ValueType, component.NameType);
        }

        return (WorldComponentWriter)scope.New(type)!;
    }
}
