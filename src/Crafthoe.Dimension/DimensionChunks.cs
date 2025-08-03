namespace Crafthoe.Dimension;

[Dimension]
public class DimensionChunks
{
    private readonly EntPtr[][][] map = new EntPtr[LevelSize * LevelSize][][];

    public bool TryGet(Vector2i cloc, out EntMut chunk)
    {
        int x1 = (cloc.X >> (LevelBits * 2)) & LevelMask;
        int y1 = (cloc.Y >> (LevelBits * 2)) & LevelMask;

        var level1 = map[(y1 << LevelBits) + x1];
        if (level1 == null)
        {
            chunk = default;
            return false;
        }

        int x2 = (cloc.X >> LevelBits) & LevelMask;
        int y2 = (cloc.Y >> LevelBits) & LevelMask;

        var level2 = level1[(y2 << LevelBits) + x2];
        if (level2 == null)
        {
            chunk = default;
            return false;
        }

        int x3 = (cloc.X) & LevelMask;
        int y3 = (cloc.Y) & LevelMask;

        chunk = level2[(y3 << LevelBits) + x3];
        return chunk != default;
    }

    public EntMut Alloc(Vector2i cloc)
    {
        if (TryGet(cloc, out var val))
            return val;

        int x1 = (cloc.X >> (LevelBits * 2)) & LevelMask;
        int y1 = (cloc.Y >> (LevelBits * 2)) & LevelMask;

        ref var level1 = ref map[(y1 << LevelBits) + x1];
        level1 ??= new EntPtr[LevelSize * LevelSize][];

        int x2 = (cloc.X >> LevelBits) & LevelMask;
        int y2 = (cloc.Y >> LevelBits) & LevelMask;

        ref var level2 = ref level1[(y2 << LevelBits) + x2];
        level2 ??= new EntPtr[LevelSize * LevelSize];

        int x3 = (cloc.X) & LevelMask;
        int y3 = (cloc.Y) & LevelMask;

        ref var chunk = ref level2[(y3 << LevelBits) + x3];

        chunk = new EntPtr()
            .IsChunk(true)
            .Cloc(cloc);

        return chunk;
    }

    public void Free(Vector2i cloc)
    {
        int x1 = (cloc.X >> (LevelBits * 2)) & LevelMask;
        int y1 = (cloc.Y >> (LevelBits * 2)) & LevelMask;

        var level1 = map[(y1 << LevelBits) + x1];
        if (level1 == null)
            return;

        int x2 = (cloc.X >> LevelBits) & LevelMask;
        int y2 = (cloc.Y >> LevelBits) & LevelMask;

        var level2 = level1[(y2 << LevelBits) + x2];
        if (level2 == null)
            return;

        int x3 = (cloc.X) & LevelMask;
        int y3 = (cloc.Y) & LevelMask;

        ref var val = ref level2[(y3 << LevelBits) + x3];
        val.Dispose();
        val = default;
    }
}
