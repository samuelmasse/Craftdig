namespace TrogloUI;

[Root]
public class RootUiDraw(RootSprites sprites, RootUiScale scale, RootUiPosition position)
{
    internal void Draw(Vector2 o, EntObj n)
    {
        DrawNode(o + n.OffsetR(), n);
        foreach (var sc in n.GetNodesR())
            Draw(o + n.OffsetR(), sc);
    }

    private void DrawNode(Vector2 o, EntObj n)
    {
        DrawFlatSurface(o, n);
        DrawTexture(o, n);
        DrawText(o, n);

        if (n.HasOnDrawF())
            n.OnDrawF()?.Invoke(o);

        if (n.HasOnFrameF())
            n.OnFrameF()?.Invoke();
    }

    private void DrawFlatSurface(Vector2 o, EntObj n)
    {
        if (!n.HasColorV() && !n.HasColorF())
            return;

        var color = Get(n.ColorV(), n.ColorF());
        if (n.SizeR() == (0, 0) || color.W == 0)
            return;

        sprites.Batch.Draw(o, n.SizeR(), color);
    }

    private void DrawTexture(Vector2 o, EntObj n)
    {
        if (!n.HasTextureV() && !n.HasTextureF())
            return;

        var texture = Get(n.TextureV(), n.TextureF());
        if (texture == null)
            return;

        var tint = Get(n.TintV(), n.TintF()) ?? Vector4.One;
        sprites.Batch.Draw(texture, o, n.SizeR(), tint);
    }

    private void DrawText(Vector2 o, EntObj n)
    {
        if (!n.HasFontV() && !n.HasFontF())
            return;

        var font = Get(n.FontV(), n.FontF());
        if (font == null)
            return;

        var fontSize = (int)(Get(n.FontSizeV(), n.FontSizeF()) * scale.Scale);
        if (fontSize <= 0)
            return;

        var text = Get(n.TextV().AsSpan(), n.TextF());
        if (text.IsEmpty)
            return;

        var textColor = Get(n.TextColorV(), n.TextColorF());
        if (textColor.W == 0)
            return;

        var alignment = Get(n.TextAlignmentV(), n.TextAlignmentF()) ?? Alignment.Center;
        var size = new Vector2(sprites.Batch.Measure(font.Size(fontSize), text), font.Size(fontSize).Metrics.Height) / scale.Scale;
        var offset = Vector2.Zero;

        var fontPadding = Get(n.FontPaddingV(), n.FontPaddingF());
        var textPadding = Get(n.TextPaddingV(), n.TextPaddingF());

        if ((alignment & (Alignment.Right | Alignment.Horizontal)) == 0)
            offset.X += fontPadding.X + textPadding.X;
        if ((alignment & (Alignment.Bottom | Alignment.Vertical)) == 0)
            offset.Y += fontPadding.Y + textPadding.Y;

        position.Align(ref offset, size, n.SizeR(), alignment);
        offset.Y += size.Y / 2;

        sprites.Batch.Write(font.Size(fontSize), text, (o + offset) * scale.Scale, textColor, scale.Scale);
    }
}
