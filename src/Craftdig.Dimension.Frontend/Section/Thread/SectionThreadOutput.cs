namespace Craftdig;

public readonly record struct SectionThreadOutput(List<BlockVertex> Buffer, Vec3i Sloc, int Revision);
