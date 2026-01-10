namespace Craftdig.Module.Frontend;

public readonly record struct BlockFaces(Ent Front, Ent Back, Ent Top, Ent Bottom, Ent Left, Ent Right)
{
    public BlockFaces(Ent all) : this(all, all, all, all, all, all) { }
}
