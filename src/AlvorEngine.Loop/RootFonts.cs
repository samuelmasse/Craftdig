namespace AlvorEngine.Loop;

[Root]
public class RootFonts(RootGlw gl)
{
    private readonly FontContext ctx = new(gl, new FontFreeTypeDriver(), new(gl));
    private readonly Dictionary<string, Font> fonts = [];

    public Font this[string path]
    {
        get
        {
            if (fonts.TryGetValue(path, out var value))
                return value;

            var font = new Font(ctx, path);
            fonts[path] = font;
            return font;
        }
    }

    internal void Pack()
    {
        foreach (var font in fonts)
            font.Value.Pack();
    }

    internal void Unload()
    {
        foreach (var font in fonts)
            font.Value.Dispose();

        ctx.Dispose();
    }
}
