namespace Crafthoe.Dimension;

public class L3Map512<T> where T : struct, IEquatable<T>
{
    private const int LevelBits = 9;
    private const int LevelSize = 1 << LevelBits;
    private const int LevelMask = LevelSize - 1;

    private readonly L3[] map = new L3[LevelSize * LevelSize];

    public T this[Vector2i index]
    {
        get
        {
            if (TryGetValue(index, out var value))
                return value;

            throw new KeyNotFoundException();
        }
        set
        {
            if (ContainsKey(index))
            {
                if (!value.Equals(default))
                    Set(index, value);
                else Remove(index);
            }
            else Insert(index, value);
        }
    }

    public bool ContainsKey(Vector2i index) => TryGetValue(index, out _);

    public bool TryGetValue(Vector2i index, out T value)
    {
        int x1 = (index.X >> (LevelBits * 2)) & LevelMask;
        int y1 = (index.Y >> (LevelBits * 2)) & LevelMask;

        var level1 = map[(y1 << LevelBits) + x1];
        if (level1.Data == null)
        {
            value = default;
            return false;
        }

        int x2 = (index.X >> LevelBits) & LevelMask;
        int y2 = (index.Y >> LevelBits) & LevelMask;

        var level2 = level1.Data[(y2 << LevelBits) + x2];
        if (level2.Data == null)
        {
            value = default;
            return false;
        }

        int x3 = (index.X) & LevelMask;
        int y3 = (index.Y) & LevelMask;

        value = level2.Data[(y3 << LevelBits) + x3];
        return !value.Equals(default);
    }

    public void Add(Vector2i index, T value)
    {
        if (ContainsKey(index))
            throw new ArgumentException();

        Insert(index, value);
    }

    public bool Remove(Vector2i index)
    {
        if (ContainsKey(index))
        {
            Delete(index);
            return true;
        }
        else return false;
    }

    private void Insert(Vector2i index, T value)
    {
        int x1 = (index.X >> (LevelBits * 2)) & LevelMask;
        int y1 = (index.Y >> (LevelBits * 2)) & LevelMask;

        ref var level1 = ref map[(y1 << LevelBits) + x1];
        level1.Data ??= new L2[LevelSize * LevelSize];

        int x2 = (index.X >> LevelBits) & LevelMask;
        int y2 = (index.Y >> LevelBits) & LevelMask;

        ref var level2 = ref level1.Data[(y2 << LevelBits) + x2];
        if (level2.Data == null)
        {
            level1.Count++;
            level2.Data ??= new T[LevelSize * LevelSize];
        }

        int x3 = (index.X) & LevelMask;
        int y3 = (index.Y) & LevelMask;

        level2.Data[(y3 << LevelBits) + x3] = value;
        level2.Count++;
    }

    private void Set(Vector2i index, T value)
    {
        int x1 = (index.X >> (LevelBits * 2)) & LevelMask;
        int y1 = (index.Y >> (LevelBits * 2)) & LevelMask;
        ref var level1 = ref map[(y1 << LevelBits) + x1];

        int x2 = (index.X >> LevelBits) & LevelMask;
        int y2 = (index.Y >> LevelBits) & LevelMask;
        ref var level2 = ref level1.Data![(y2 << LevelBits) + x2];

        int x3 = (index.X) & LevelMask;
        int y3 = (index.Y) & LevelMask;
        level2.Data![(y3 << LevelBits) + x3] = value;
    }

    private void Delete(Vector2i index)
    {
        int x1 = (index.X >> (LevelBits * 2)) & LevelMask;
        int y1 = (index.Y >> (LevelBits * 2)) & LevelMask;

        ref var level1 = ref map[(y1 << LevelBits) + x1];
        int x2 = (index.X >> LevelBits) & LevelMask;
        int y2 = (index.Y >> LevelBits) & LevelMask;

        ref var level2 = ref level1.Data![(y2 << LevelBits) + x2];
        int x3 = (index.X) & LevelMask;
        int y3 = (index.Y) & LevelMask;

        level2.Data![(y3 << LevelBits) + x3] = default;
        level2.Count--;

        if (level2.Count == 0)
        {
            level1.Count--;
            level2 = default;
        }

        if (level1.Count == 0)
            level1 = default;
    }

    public struct L3
    {
        public int Count;
        public L2[]? Data;
    }

    public struct L2
    {
        public int Count;
        public T[]? Data;
    }
}
