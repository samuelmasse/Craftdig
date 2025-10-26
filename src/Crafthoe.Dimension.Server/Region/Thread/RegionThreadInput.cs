namespace Crafthoe.Dimension;

public record struct RegionThreadInput(Vector3i Sloc, RegionThreadInputType Type, Memory<Ent> Blocks);
