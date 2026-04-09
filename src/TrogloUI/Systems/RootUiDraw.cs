namespace TrogloUI;

[Root]
public class RootUiDraw(RootSprites sprites, RootUiScale scale, RootUiPosition position, RootUiClipping clipping)
{
    internal void Draw(Ent n)
    {
        var clip = sprites.Batch.Clip;
        sprites.Batch.Clip = clipping.IntersectClips(clip, new(n.PositionR, n.PositionR + n.SizeR));

        DrawNode(n);
        foreach (var sc in n.NodesR.Span)
            Draw(sc);

        sprites.Batch.Clip = clip;
    }

    private void DrawNode(Ent n)
    {
        DrawFlatSurface(n);
        DrawTexture(n);
        DrawText(n);

        n.OnDrawFV.Resolve()?.Invoke();
        n.OnFrameFV.Resolve()?.Invoke();
    }

    private void DrawFlatSurface(Ent n)
    {
        var color = n.ColorFV.Resolve();
        if (n.SizeR == (0, 0) || color.W == 0)
            return;

        sprites.Batch.Draw(n.PositionR, n.SizeR, color);
    }

    private void DrawTexture(Ent n)
    {
        var texture = n.TextureFV.Resolve();
        if (texture == null)
            return;

        var color = n.TextureColorFV.Resolve() ?? Vector4.One;
        var subPosition = n.TextureSubPositionFV.Resolve() ?? Vector2.Zero;
        var subSize = n.TextureSubSizeFV.Resolve() ?? texture.Size;
        var rotation = n.TextureRotationFV.Resolve() ?? SpriteBatchRotation.None;
        var flip = n.TextureFlipFV.Resolve() ?? SpriteBatchFlip.None;

        sprites.Batch.Draw(texture, n.PositionR, n.SizeR, subPosition, subSize, color, rotation, flip);
    }

    private void DrawText(Ent n)
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

        sprites.Batch.Write(font.Size(fontSize), text, (n.PositionR + offset) * scale.Scale, textColor, scale.Scale);
    }
}
