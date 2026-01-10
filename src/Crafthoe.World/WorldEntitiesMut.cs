namespace Crafthoe.World;

[World]
public class WorldEntitiesMut(WorldEntArena entArena)
{
    private const int LevelSize = 1 << 16;
    private readonly EntMut[][][][] dict = new EntMut[LevelSize][][][];

    public EntMut this[ulong worldId]
    {
        get
        {
            int i0 = (int)((worldId >> 48) & 0xFFFF);
            int i1 = (int)((worldId >> 32) & 0xFFFF);
            int i2 = (int)((worldId >> 16) & 0xFFFF);
            int i3 = (int)(worldId & 0xFFFF);

            var level1 = dict[i0] ?? EnsureLevel1(i0);
            var level2 = level1[i1] ?? EnsureLevel2(level1, i1);
            var level3 = level2[i2] ?? EnsureLevel3(level2, i2);

            if (level3[i3] == default)
            {
                lock (level3)
                {
                    if (level3[i3] == default)
                        level3[i3] = entArena.Arena.Alloc().WorldId(worldId);
                }
            }

            return level3[i3];
        }
    }

    private EntMut[][][] EnsureLevel1(int index)
    {
        lock (dict)
        {
            ref var level = ref dict[index];
            level ??= new EntMut[LevelSize][][];
            return level;
        }
    }

    private static EntMut[][] EnsureLevel2(EntMut[][][] level1, int index)
    {
        lock (level1)
        {
            ref var level = ref level1[index];
            level ??= new EntMut[LevelSize][];
            return level;
        }
    }

    private static EntMut[] EnsureLevel3(EntMut[][] level2, int index)
    {
        lock (level2)
        {
            ref var level = ref level2[index];
            level ??= new EntMut[LevelSize];
            return level;
        }
    }
}
