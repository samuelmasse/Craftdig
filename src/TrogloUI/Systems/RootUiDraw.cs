namespace TrogloUI;

[Root]
public class RootUiDraw(RootSprites sprites, RootUiScale scale, RootUiPosition position, RootUiClipping clipping)
{
    internal void Draw(Vector2 o, EntMut n)
    {
        var clip = sprites.Batch.Clip;
        sprites.Batch.Clip = clipping.IntersectClips(clip, new(o + n.OffsetR, o + n.OffsetR + n.SizeR));

        DrawNode(o + n.OffsetR, n);
        foreach (var sc in n.NodesR.Span)
            Draw(o + n.OffsetR, sc);

        sprites.Batch.Clip = clip;
    }

    private void DrawNode(Vector2 o, EntMut n)
    {
        n.DrawOffsetR = o;

        DrawFlatSurface(o, n);
        DrawTexture(o, n);
        DrawText(o, n);

        n.OnDrawFV.Resolve()?.Invoke(o);
        n.OnFrameFV.Resolve()?.Invoke();
    }

    private void DrawFlatSurface(Vector2 o, EntMut n)
    {
        var color = n.ColorFV.Resolve();
        if (n.SizeR == (0, 0) || color.W == 0)
            return;

        sprites.Batch.Draw(o, n.SizeR, color);
    }

    private void DrawTexture(Vector2 o, EntMut n)
    {
        var texture = n.TextureFV.Resolve();
        if (texture == null)
            return;

        var tint = n.TintFV.Resolve() ?? Vector4.One;
        sprites.Batch.Draw(texture, o, n.SizeR, tint);
    }

    private void DrawText(Vector2 o, EntMut n)
    {
        var font = n.FontFV.Resolve();
        if (font == null)
            return;

        var fontSize = (int)(n.FontSizeFV.Resolve() * scale.Scale);
        if (fontSize <= 0)
            return;

        var text = n.TextFV.Resolve();
        if (text.IsEmpty)
            return;

        var textColor = n.TextColorFV.Resolve();
        if (textColor.W == 0)
            return;

        var alignment = n.TextAlignmentFV.Resolve() ?? Alignment.Center;
        var size = new Vector2(sprites.Batch.Measure(font.Size(fontSize), text), font.Size(fontSize).Metrics.Height) / scale.Scale;
        var offset = Vector2.Zero;

        var fontPadding = n.FontPaddingFV.Resolve();
        var textPadding = n.TextPaddingFV.Resolve();

        if ((alignment & (Alignment.Right | Alignment.Horizontal)) == 0)
            offset.X += fontPadding.X + textPadding.X;
        if ((alignment & (Alignment.Bottom | Alignment.Vertical)) == 0)
            offset.Y += fontPadding.Y + textPadding.Y;

        offset = position.Align(offset, size, n.SizeR, alignment);
        offset.Y += size.Y / 2;

        sprites.Batch.Write(font.Size(fontSize), text, (o + offset) * scale.Scale, textColor, scale.Scale);
    }
}
