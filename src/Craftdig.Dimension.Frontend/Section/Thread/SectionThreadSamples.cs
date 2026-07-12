namespace Craftdig.Dimension.Frontend;

public class SectionThreadSamples
{
    public Ent[] Blocks { get; }
    public LightLevels[] Lights { get; }

    public SectionThreadSamples()
    {
        int size = SectionSize + 2;
        int volume = size * size * size;
        Blocks = new Ent[volume];
        Lights = new LightLevels[volume];
    }
}
