namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionComponentWriters(WorldComponentIndices indices, DimensionScope scope)
{
    private DimensionComponentWriter[] writers = [];

    public DimensionComponentWriter this[int index]
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

    private DimensionComponentWriter New(EntComponent component)
    {
        Type? type;

        if (component.ValueType.IsArray)
            type = typeof(DimensionComponentArrayWriter<,>).MakeGenericType(component.ValueType.GetElementType()!, component.NameType);
        else
        {
            if (component.ValueType == typeof(Ent))
                type = typeof(DimensionComponentEntWriter<>).MakeGenericType(component.NameType);
            else type = typeof(DimensionComponentWriter<,>).MakeGenericType(component.ValueType, component.NameType);
        }

        return (DimensionComponentWriter)scope.New(type)!;
    }
}
