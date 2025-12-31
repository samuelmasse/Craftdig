namespace Craftdig.Dimension;

public class ValueChanges<T>
{
    private readonly Dictionary<Vector3i, int> indices = [];
    private readonly List<ValueChange<T>> changes = [];

    public ReadOnlySpan<ValueChange<T>> Span => CollectionsMarshal.AsSpan(changes);

    public void Add(Vector3i loc, T prev)
    {
        if (indices.TryGetValue(loc, out int index))
            changes[index] = new(loc, prev);
        else
        {
            indices.Add(loc, changes.Count);
            changes.Add(new(loc, prev));
        }
    }

    public void Clear()
    {
        indices.Clear();
        changes.Clear();
    }
}
