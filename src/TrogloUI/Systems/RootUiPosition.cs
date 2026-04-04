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

            var alignment = GetAlignment(c);
            if ((alignment & (Alignment.Right | Alignment.Horizontal)) == 0)
                ce.OffsetR += (n.PaddingR.X, 0);
            if ((alignment & (Alignment.Bottom | Alignment.Vertical)) == 0)
                ce.OffsetR += (0, n.PaddingR.Y);
        }

        var innerLayout = Get(n.InnerLayoutV, n.InnerLayoutFDelegate);
        var innerSpacing = Get(n.InnerSpacingV, n.InnerSpacingFDelegate);

        if (innerLayout == InnerLayout.VerticalList)
        {
            float y = 0;

            foreach (var c in n.NodesR.Span)
            {
                var ce = c;
                if (IsFloating(c))
                    continue;

                ce.OffsetR += (0, y);
                y += c.SizeR.Y;
                y += innerSpacing;
            }
        }
        else if (innerLayout == InnerLayout.HorizontalList)
        {
            float x = 0;

            foreach (var c in n.NodesR.Span)
            {
                var ce = c;
                if (IsFloating(c))
                    continue;

                ce.OffsetR += (x, 0);
                x += c.SizeR.X;
                x += innerSpacing;
            }
        }
    }

    private void PositionNode(Vector2 s, EntMut n)
    {
        n.OffsetR = default;
        n.OffsetR += Get(n.OffsetV, n.OffsetFDelegate);
        PositionTextRelative(n);
        PositionAlignement(s, n);
        PositionMultiplier(n);
    }

    private void PositionAlignement(Vector2 s, EntMut n)
    {
        var alignment = GetAlignment(n);
        n.OffsetR = Align(n.OffsetR, n.SizeR, s, alignment);
    }

    private void PositionTextRelative(EntMut n)
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

        var size = new Vector2(sprites.Batch.Measure(font.Size(fontSize), text), font.Size(fontSize).Metrics.Height) / scale.Scale;
        n.OffsetR += Get(n.OffsetTextRelativeV, n.OffsetTextRelativeFDelegate) * size;
    }

    private void PositionMultiplier(EntMut n)
    {
        if (!n.HasOffsetMultiplierV && !n.HasOffsetMultiplierF)
            return;

        var multiplier = Get(n.OffsetMultiplierV, n.OffsetMultiplierFDelegate);

        n.OffsetR = (
            (float)Math.Round(n.OffsetR.X / multiplier) * multiplier,
            (float)Math.Round(n.OffsetR.Y / multiplier) * multiplier);
    }

    private Alignment GetAlignment(EntMut n)
    {
        if (!n.HasAlignmentV && !n.HasAlignmentF)
            return Alignment.None;

        return Get(n.AlignmentV, n.AlignmentFDelegate);
    }

    private bool IsFloating(EntMut n)
    {
        if (!n.HasIsFloatingV && !n.HasIsFloatingF)
            return false;

        return Get(n.IsFloatingV, n.IsFloatingFDelegate);
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
