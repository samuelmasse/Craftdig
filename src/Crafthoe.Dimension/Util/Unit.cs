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
    public static Vector3d Swizzle(this Vector3d pos) => (pos.X, pos.Z, pos.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector3 Swizzle(this Vector3 pos) => (pos.X, pos.Z, pos.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector3i Swizzle(this Vector3i loc) => (loc.X, loc.Z, loc.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector3i ToLoc(this Vector3d pos) =>
        ((int)Math.Floor(pos.X), (int)Math.Floor(pos.Y), (int)Math.Floor(pos.Z));

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2i ToCloc(this Vector2i loc) =>
        (loc.X >> SectionBits, loc.Y >> SectionBits);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector3i ToSloc(this Vector3i loc) =>
        (loc.X >> SectionBits, loc.Y >> SectionBits, loc.Z >> SectionBits);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2i ToRloc(this Vector2i cloc) =>
        (cloc.X >> RegionBits, cloc.Y >> RegionBits);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static int ToInnerIndex(this Vector3i loc) =>
        (loc.Z << (SectionBits * 2)) + ((loc.Y & SectionMask) << SectionBits) + (loc.X & SectionMask);
}
