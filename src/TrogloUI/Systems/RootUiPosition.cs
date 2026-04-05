namespace TrogloUI;

[Root]
public class RootUiPosition(RootSprites sprites, RootUiScale scale)
{
    internal void Position(Vector2 s, EntMut n)
    {
        PositionNode(s, n);
        foreach (var c in n.NodesR.Span)
        {
            var ce = c;
            Position(n.SizeR, c);

            var alignment = c.AlignmentFV.Resolve();
            if ((alignment & (Alignment.Right | Alignment.Horizontal)) == 0)
                ce.OffsetR += (n.PaddingR.X, 0);
            if ((alignment & (Alignment.Bottom | Alignment.Vertical)) == 0)
                ce.OffsetR += (0, n.PaddingR.Y);
        }

        var innerLayout = n.InnerLayoutFV.Resolve();
        var innerSpacing = n.InnerSpacingFV.Resolve();
        var innerScrollOffset = n.InnerScrollOffsetFV.Resolve();

        if (innerLayout == InnerLayout.VerticalList)
        {
            float y = innerScrollOffset.Y;

            foreach (var c in n.NodesR.Span)
            {
                var ce = c;
                if (c.IsFloatingFV.Resolve())
                    continue;

                y += c.MarginR.Y;
                ce.OffsetR += (c.MarginR.X, y);
                y += c.SizeR.Y + c.MarginR.W;
                y += innerSpacing;
            }
        }
        else if (innerLayout == InnerLayout.HorizontalList)
        {
            float x = innerScrollOffset.X;

            foreach (var c in n.NodesR.Span)
            {
                var ce = c;
                if (c.IsFloatingFV.Resolve())
                    continue;

                x += c.MarginR.X;
                ce.OffsetR += (x, c.MarginR.Y);
                x += c.SizeR.X + c.MarginR.Z;
                x += innerSpacing;
            }
        }
    }

    private void PositionNode(Vector2 s, EntMut n)
    {
        n.OffsetR = default;
        n.OffsetR += n.OffsetFV.Resolve();
        PositionTextRelative(n);
        PositionAlignement(s, n);
        PositionMultiplier(n);
    }

    private void PositionAlignement(Vector2 s, EntMut n)
    {
        var alignment = n.AlignmentFV.Resolve();
        n.OffsetR = Align(n.OffsetR, n.SizeR, s, alignment);
    }

    private void PositionTextRelative(EntMut n)
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

        var size = new Vector2(sprites.Batch.Measure(font.Size(fontSize), text), font.Size(fontSize).Metrics.Height) / scale.Scale;
        n.OffsetR += n.OffsetTextRelativeFV.Resolve() * size;
    }

    private void PositionMultiplier(EntMut n)
    {
        var multiplier = n.OffsetMultiplierFV.Resolve();
        if (multiplier == 0)
            return;

        n.OffsetR = (
            (float)Math.Round(n.OffsetR.X / multiplier) * multiplier,
            (float)Math.Round(n.OffsetR.Y / multiplier) * multiplier);
    }

    internal Vector2 Align(Vector2 val, Vector2 size, Vector2 parent, Alignment alignment)
    {
        if ((alignment & Alignment.Horizontal) != 0)
            val.X += parent.X / 2 - size.X / 2;
        if ((alignment & Alignment.Vertical) != 0)
            val.Y += parent.Y / 2 - size.Y / 2;

        if ((alignment & Alignment.Right) != 0)
            val.X += parent.X - size.X;
        if ((alignment & Alignment.Bottom) != 0)
            val.Y += parent.Y - size.Y;

        return val;
    }
}
