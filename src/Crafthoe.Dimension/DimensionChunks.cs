namespace Crafthoe.Dimension;

[Dimension]
public class DimensionChunks
{
    private readonly Entity?[][][] map = new Entity?[LevelSize * LevelSize][][];

    public Entity? this[Vector2i cloc]
    {
        get
        {
            int x1 = (cloc.X >> (LevelBits * 2)) & LevelMask;
            int y1 = (cloc.Y >> (LevelBits * 2)) & LevelMask;

            var level1 = map[(y1 << LevelBits) + x1];
            if (level1 == null)
                return null;

            int x2 = (cloc.X >> LevelBits) & LevelMask;
            int y2 = (cloc.Y >> LevelBits) & LevelMask;

            var level2 = level1[(y2 << LevelBits) + x2];
            if (level2 == null)
                return null;

            int x3 = (cloc.X) & LevelMask;
            int y3 = (cloc.Y) & LevelMask;

            var chunk = level2[(y3 << LevelBits) + x3];
            if (chunk == null)
                return null;

            return chunk;
        }
    }

    public void Alloc(Vector2i cloc)
    {
        if (this[cloc] != null)
            return;

        int x1 = (cloc.X >> (LevelBits * 2)) & LevelMask;
        int y1 = (cloc.Y >> (LevelBits * 2)) & LevelMask;

        ref var level1 = ref map[(y1 << LevelBits) + x1];
        level1 ??= new Entity?[LevelSize * LevelSize][];

        int x2 = (cloc.X >> LevelBits) & LevelMask;
        int y2 = (cloc.Y >> LevelBits) & LevelMask;

        ref var level2 = ref level1[(y2 << LevelBits) + x2];
        level2 ??= new Entity?[LevelSize * LevelSize];

        int x3 = (cloc.X) & LevelMask;
        int y3 = (cloc.Y) & LevelMask;

        ref var chunk = ref level2[(y3 << LevelBits) + x3];

        chunk = new Entity()
            .IsChunk(true)
            .ChunkLocation(cloc);
    }
}
