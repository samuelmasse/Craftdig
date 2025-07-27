namespace AlvorEngine.Loop;

[Root]
public class RootRoboto
{
    private readonly Font font;

    public FontSize this[int size] => font.Size(size);
    public Font Font => font;

    public RootRoboto(RootFonts fonts)
    {
        var assembly = typeof(RootRoboto).Assembly;
        string[] resourceNames = typeof(RootRoboto).Assembly.GetManifestResourceNames();

        using var stream = assembly.GetManifestResourceStream(resourceNames.First(x => x.Contains("Roboto")))!;
        using var ms = new MemoryStream();
        stream.CopyTo(ms);

        byte[] data = ms.ToArray();
        font = fonts.Open(new() { Data = data });
    }
}
