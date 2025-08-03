namespace AlvorEngine;

public struct LazySortedList<TKey, TValue> where TKey : notnull
{
    private SortedList<TKey, TValue> list;

    public TValue this[TKey key]
    {
        get
        {
            EnsureNotNull();
            return list[key];
        }
        set
        {
            EnsureNotNull();
            list[key] = value;
        }
    }

    public IList<TKey> Keys
    {
        get
        {
            EnsureNotNull();
            return list.Keys;
        }
    }

    public IList<TValue> Values
    {
        get
        {
            EnsureNotNull();
            return list.Values;
        }
    }

    public readonly int Count
    {
        get
        {
            if (list == null)
                return 0;
            else return list.Count;
        }
    }

    public void Add(TKey key, TValue value)
    {
        EnsureNotNull();
        list.Add(key, value);
    }

    public readonly void Clear()
    {
        if (list == null)
            return;
        else list.Clear();
    }

    public readonly bool ContainsKey(TKey key)
    {
        if (list == null)
            return false;
        else return list.ContainsKey(key);
    }

    public readonly bool Remove(TKey key)
    {
        if (list == null)
            return false;
        else return list.Remove(key);
    }

    public readonly bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (list == null)
        {
            value = default;
            return false;
        }
        else return list.TryGetValue(key, out value);
    }

    private void EnsureNotNull() => list ??= [];
}
