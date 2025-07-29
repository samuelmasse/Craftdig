namespace Crafthoe.Dimension;

public static class Unit
{
    public const int LevelBits = 9;
    public const int LevelSize = 1 << LevelBits;
    public const int LevelMask = LevelSize - 1;

    public const int SectionBits = 4;
    public const int SectionSize = 1 << SectionBits;
    public const int SectionMask = SectionSize - 1;

    public const int HeightBits = 9;
    public const int HeightSize = 1 << HeightBits;
    public const int HeightMask = HeightSize - 1;

    public static Vector3i ChunkSize => new(SectionSize, SectionSize, HeightSize);
}
