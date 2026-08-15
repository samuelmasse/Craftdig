namespace Craftdig;

public abstract class EntSyncCatalog
{
    public const int ComponentsPerMask = sizeof(ulong) * 8;

    private readonly EntSyncComponent[] components;

    public ReadOnlySpan<EntSyncComponent> Components => components;
    public ulong SchemaHash { get; }
    public EntSyncComponent this[int ordinal] => components[ordinal];

    protected EntSyncCatalog(Type groupType)
    {
        var sourceType = (Type)groupType.GetProperty(nameof(IComponentGroup.SourceInterfaceType))!.GetValue(null)!;
        var entries = new List<(PropertyInfo Property, Type NameType, SyncedAttribute Attribute)>();

        foreach (var property in sourceType.GetProperties())
        {
            var attribute = property.GetCustomAttribute<SyncedAttribute>();
            if (attribute != null)
                entries.Add((property, groupType.GetNestedType(property.Name)!, attribute));
        }

        entries.Sort((a, b) => string.CompareOrdinal(a.NameType.FullName, b.NameType.FullName));
        components = new EntSyncComponent[entries.Count];

        for (ushort i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            components[i] = Create(
                new(entry.Property.PropertyType, entry.NameType),
                i,
                entry.Attribute.Audience,
                entry.Attribute.MaximumCount);
        }

        SchemaHash = Hash(components);
    }

    public EntSyncComponent this[EntComponent component]
    {
        get
        {
            int index = 0;
            while (components[index].Component != component)
                index++;
            return components[index];
        }
    }

    private EntSyncComponent Create(
        EntComponent component,
        ushort ordinal,
        EntSyncAudience audience,
        int maximumCount)
    {
        Type type;
        if (component.ValueType == typeof(Ent))
            type = typeof(EntSyncModuleComponent<>).MakeGenericType(component.NameType);
        else if (component.ValueType == typeof(EntMutIdx))
            type = typeof(EntSyncEntComponent<>).MakeGenericType(component.NameType);
        else if (component.ValueType == typeof(Ent[]))
            type = typeof(EntSyncModuleArrayComponent<>).MakeGenericType(component.NameType);
        else if (component.ValueType == typeof(EntMutIdx[]))
            type = typeof(EntSyncEntArrayComponent<>).MakeGenericType(component.NameType);
        else if (component.ValueType.IsArray)
            type = typeof(EntSyncUnmanagedArrayComponent<,>).MakeGenericType(component.ValueType.GetElementType()!, component.NameType);
        else
            type = typeof(EntSyncUnmanagedComponent<,>).MakeGenericType(component.ValueType, component.NameType);

        bool isArray = component.ValueType.IsArray;
        if (isArray && maximumCount <= 0)
            throw new InvalidOperationException($"Synced array {component} requires an explicit maximum count.");
        if (!isArray && maximumCount != 0)
            throw new InvalidOperationException($"Synced value {component} cannot declare a maximum count.");

        return (EntSyncComponent)Activator.CreateInstance(type, component, ordinal, audience, maximumCount)!;
    }

    private ulong Hash(ReadOnlySpan<EntSyncComponent> values)
    {
        ulong hash = 14695981039346656037UL;
        foreach (var value in values)
        {
            AddString(value.Component.NameType.FullName!);
            AddString(value.Component.ValueType.FullName!);
            AddByte((byte)value.Audience);
            AddInt32(value.ElementSize);
            AddInt32(value.MaximumCount);
        }
        return hash;

        void AddString(string value)
        {
            foreach (char c in value)
            {
                AddByte((byte)c);
                AddByte((byte)(c >> 8));
            }
            AddByte(0xFF);
        }

        void AddByte(byte value)
        {
            hash ^= value;
            hash *= 1099511628211UL;
        }

        void AddInt32(int value)
        {
            AddByte((byte)value);
            AddByte((byte)(value >> 8));
            AddByte((byte)(value >> 16));
            AddByte((byte)(value >> 24));
        }
    }
}

[World]
public class WorldEntSyncCatalog() : EntSyncCatalog(typeof(WorldComponents));

[Dimension]
public class DimensionEntSyncCatalog() : EntSyncCatalog(typeof(DimensionComponents));
