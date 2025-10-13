namespace TrogloUI;

[Root]
public class RootUiPosition(RootSprites sprites, RootUiScale scale)
{
    internal void Position(Vector2 s, EntObj n)
    {
        PositionNode(s, n);
        foreach (var c in n.GetNodesR())
        {
            Position(n.SizeR(), c);

            var alignment = GetAlignment(c);
            if ((alignment & (Alignment.Right | Alignment.Horizontal)) == 0)
                c.OffsetR().X += n.PaddingR().X;
            if ((alignment & (Alignment.Bottom | Alignment.Vertical)) == 0)
                c.OffsetR().Y += n.PaddingR().Y;
        }

        var innerLayout = Get(n.InnerLayoutV(), n.InnerLayoutF());
        var innerSpacing = Get(n.InnerSpacingV(), n.InnerSpacingF());

        if (innerLayout == InnerLayout.VerticalList)
        {
            float y = 0;

            foreach (var c in n.GetNodesR())
            {
                if (IsFloating(c))
                    continue;

                c.OffsetR().Y += y;
                y += c.SizeR().Y;
                y += innerSpacing;
            }
        }
        else if (innerLayout == InnerLayout.HorizontalList)
        {
            float x = 0;

            foreach (var c in n.GetNodesR())
            {
                if (IsFloating(c))
                    continue;

                c.OffsetR().X += x;
                x += c.SizeR().X;
                x += innerSpacing;
            }
        }
    }

    private void PositionNode(Vector2 s, EntObj n)
    {
        n.OffsetR() = default;
        n.OffsetR() += Get(n.OffsetV(), n.OffsetF());
        PositionTextRelative(n);
        PositionAlignement(s, n);
        PositionMultiplier(n);
    }

    private void PositionAlignement(Vector2 s, EntObj n)
    {
        var alignment = GetAlignment(n);
        Align(ref n.OffsetR(), n.SizeR(), s, alignment);
    }

    private void PositionTextRelative(EntObj n)
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

        var size = new Vector2(sprites.Batch.Measure(font.Size(fontSize), text), font.Size(fontSize).Metrics.Height) / scale.Scale;
        n.OffsetR() += Get(n.OffsetTextRelativeV(), n.OffsetTextRelativeF()) * size;
    }

    private void PositionMultiplier(EntObj n)
    {
        if (!n.HasOffsetMultiplierV() && !n.HasOffsetMultiplierF())
            return;

        var multiplier = Get(n.OffsetMultiplierV(), n.OffsetMultiplierF());

        n.OffsetR() = (
            (float)Math.Round(n.OffsetR().X / multiplier) * multiplier,
            (float)Math.Round(n.OffsetR().Y / multiplier) * multiplier);
    }

    private Alignment GetAlignment(EntObj n)
    {
        if (!n.HasAlignmentV() && !n.HasAlignmentF())
            return Alignment.None;

        return Get(n.AlignmentV(), n.AlignmentF());
    }

    private bool IsFloating(EntObj n)
    {
        if (!n.HasIsFloatingV() && !n.HasIsFloatingF())
            return false;

        return Get(n.IsFloatingV(), n.IsFloatingF());
    }

    internal void Align(ref Vector2 val, Vector2 size, Vector2 parent, Alignment alignment)
    {
        if ((alignment & Alignment.Horizontal) != 0)
            val.X += parent.X / 2 - size.X / 2;
        if ((alignment & Alignment.Vertical) != 0)
            val.Y += parent.Y / 2 - size.Y / 2;

        if ((alignment & Alignment.Right) != 0)
            val.X += parent.X - size.X;
        if ((alignment & Alignment.Bottom) != 0)
            val.Y += parent.Y - size.Y;
    }
}
