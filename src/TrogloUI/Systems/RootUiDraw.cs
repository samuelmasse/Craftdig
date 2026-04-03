namespace TrogloUI;

[Root]
public class RootUiDraw(RootSprites sprites, RootUiScale scale, RootUiPosition position, RootUiClipping clipping)
{
    internal void Draw(Vector2 o, EntObj n)
    {
        var clip = sprites.Batch.Clip;
        sprites.Batch.Clip = clipping.IntersectClips(clip, new(o + n.OffsetR, o + n.OffsetR + n.SizeR));

        DrawNode(o + n.OffsetR, n);
        foreach (var sc in n.NodesR.Span)
            Draw(o + n.OffsetR, sc);

        sprites.Batch.Clip = clip;
    }

    private void DrawNode(Vector2 o, EntObj n)
    {
        DrawFlatSurface(o, n);
        DrawTexture(o, n);
        DrawText(o, n);

        if (n.HasOnDrawF)
            n.OnDrawFDelegate?.Invoke(o);

        if (n.HasOnFrameF)
            n.OnFrameFDelegate?.Invoke();
    }

    private void DrawFlatSurface(Vector2 o, EntObj n)
    {
        if (!n.HasColorV && !n.HasColorF)
            return;

        var color = Get(n.ColorV, n.ColorFDelegate);
        if (n.SizeR == (0, 0) || color.W == 0)
            return;

        sprites.Batch.Draw(o, n.SizeR, color);
    }

    private void DrawTexture(Vector2 o, EntObj n)
    {
        if (!n.HasTextureV && !n.HasTextureF)
            return;

        var texture = Get(n.TextureV, n.TextureFDelegate);
        if (texture == null)
            return;

        var tint = Get(n.TintV, n.TintFDelegate) ?? Vector4.One;
        sprites.Batch.Draw(texture, o, n.SizeR, tint);
    }

    private void DrawText(Vector2 o, EntObj n)
    {
        if (!n.HasFontV && !n.HasFontF)
            return;

        var font = Get(n.FontV, n.FontFDelegate);
        if (font == null)
            return;

        var fontSize = (int)(Get(n.FontSizeV, n.FontSizeFDelegate) * scale.Scale);
        if (fontSize <= 0)
            return;

        var text = Get(n.TextV.AsSpan(), n.TextFDelegate);
        if (text.IsEmpty)
            return;

        var textColor = Get(n.TextColorV, n.TextColorFDelegate);
        if (textColor.W == 0)
            return;

        var alignment = Get(n.TextAlignmentV, n.TextAlignmentFDelegate) ?? Alignment.Center;
        var size = new Vector2(sprites.Batch.Measure(font.Size(fontSize), text), font.Size(fontSize).Metrics.Height) / scale.Scale;
        var offset = Vector2.Zero;

        var fontPadding = Get(n.FontPaddingV, n.FontPaddingFDelegate);
        var textPadding = Get(n.TextPaddingV, n.TextPaddingFDelegate);

        if ((alignment & (Alignment.Right | Alignment.Horizontal)) == 0)
            offset.X += fontPadding.X + textPadding.X;
        if ((alignment & (Alignment.Bottom | Alignment.Vertical)) == 0)
            offset.Y += fontPadding.Y + textPadding.Y;

        offset = position.Align(offset, size, n.SizeR, alignment);
        offset.Y += size.Y / 2;

        sprites.Batch.Write(font.Size(fontSize), text, (o + offset) * scale.Scale, textColor, scale.Scale);
    }
}
