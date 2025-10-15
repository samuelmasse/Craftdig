namespace Crafthoe.Dimension;

public class L3Map512<T> where T : struct
{
    private const int LevelBits = 9;
    private const int LevelSize = 1 << LevelBits;
    private const int LevelMask = LevelSize - 1;

    private readonly T[][][] map = new T[LevelSize * LevelSize][][];

    public ref T this[Vector2i index]
    {
        get
        {
            int x1 = (index.X >> (LevelBits * 2)) & LevelMask;
            int y1 = (index.Y >> (LevelBits * 2)) & LevelMask;

            ref var level1 = ref map[(y1 << LevelBits) + x1];
            level1 ??= new T[LevelSize * LevelSize][];

            int x2 = (index.X >> LevelBits) & LevelMask;
            int y2 = (index.Y >> LevelBits) & LevelMask;

            ref var level2 = ref level1[(y2 << LevelBits) + x2];
            level2 ??= new T[LevelSize * LevelSize];

            int x3 = (index.X) & LevelMask;
            int y3 = (index.Y) & LevelMask;

            return ref level2[(y3 << LevelBits) + x3];
        }
    }

    public bool TryGet(Vector2i index, out T chunk)
    {
        int x1 = (index.X >> (LevelBits * 2)) & LevelMask;
        int y1 = (index.Y >> (LevelBits * 2)) & LevelMask;

        var level1 = map[(y1 << LevelBits) + x1];
        if (level1 == null)
        {
            chunk = default;
            return false;
        }

        int x2 = (index.X >> LevelBits) & LevelMask;
        int y2 = (index.Y >> LevelBits) & LevelMask;

        var level2 = level1[(y2 << LevelBits) + x2];
        if (level2 == null)
        {
            chunk = default;
            return false;
        }

        int x3 = (index.X) & LevelMask;
        int y3 = (index.Y) & LevelMask;

        chunk = level2[(y3 << LevelBits) + x3];
        return true;
    }
}
