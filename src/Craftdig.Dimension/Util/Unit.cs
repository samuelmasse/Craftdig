namespace Craftdig.Dimension;

public static class Unit
{
    public const int SectionBits = 4;
    public const int SectionSize = 1 << SectionBits;
    public const int SectionMask = SectionSize - 1;
    public const int SectionHeight = HeightSize / SectionSize;
    public const int SectionVolume = SectionSize * SectionSize * SectionSize;

    public const int HeightBits = 9;
    public const int HeightSize = 1 << HeightBits;
    public const int HeightMask = HeightSize - 1;

    public const int ChunkVolume = HeightSize * SectionSize * SectionSize;

    public const int RegionBits = 5;
    public const int RegionSize = 1 << RegionBits;
    public const int RegionMask = RegionSize - 1;
    public const int RegionVolume = RegionSize * RegionSize * SectionHeight;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vec3d Swizzle(this Vec3d pos) => (pos.X, pos.Z, pos.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vec3 Swizzle(this Vec3 pos) => (pos.X, pos.Z, pos.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vec3i Swizzle(this Vec3i loc) => (loc.X, loc.Z, loc.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vec3i ToLoc(this Vec3d pos) =>
        ((int)Math.Floor(pos.X), (int)Math.Floor(pos.Y), (int)Math.Floor(pos.Z));

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vec2i ToCloc(this Vec2i loc) =>
        (loc.X >> SectionBits, loc.Y >> SectionBits);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vec3i ToSloc(this Vec3i loc) =>
        (loc.X >> SectionBits, loc.Y >> SectionBits, loc.Z >> SectionBits);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vec2i ToRloc(this Vec2i cloc) =>
        (cloc.X >> RegionBits, cloc.Y >> RegionBits);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static int ToInnerIndex(this Vec3i loc) =>
        (loc.Z << (SectionBits * 2)) + ((loc.Y & SectionMask) << SectionBits) + (loc.X & SectionMask);
}
