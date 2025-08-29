namespace AlvorEngine;

public struct LazyList<T> : IList<T>, IReadOnlyList<T>
{
    private List<T> list;

    public T this[int index]
    {
        get
        {
            EnsureNotNull();
            return list[index];
        }
        set
        {
            EnsureNotNull();
            list[index] = value;
        }
    }

    public readonly int Count => list == null ? 0 : list.Count;
    public readonly bool IsReadOnly => false;

    public void Add(T item)
    {
        EnsureNotNull();
        list.Add(item);
    }

    public readonly void Clear() => list?.Clear();

    public readonly bool Contains(T item) => list?.Contains(item) == true;

    public void CopyTo(T[] array, int arrayIndex)
    {
        EnsureNotNull();
        list.CopyTo(array, arrayIndex);
    }

    public List<T>.Enumerator GetEnumerator()
    {
        EnsureNotNull();
        return list.GetEnumerator();
    }

    public readonly int IndexOf(T item)
    {
        if (list == null)
            return -1;
        else return list.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        EnsureNotNull();
        list.Insert(index, item);
    }

    public readonly bool Remove(T item)
    {
        if (list == null)
            return false;

        return list.Remove(item);
    }

    public void RemoveAt(int index)
    {
        EnsureNotNull();
        list.RemoveAt(index);
    }

    private void EnsureNotNull() => list ??= [];

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
