namespace Craftdig.Dimension;

public record struct SectionBlocks
{
    public Ent Uniform;
    public Memory<Ent> Data;
    public int Alloc;
}
