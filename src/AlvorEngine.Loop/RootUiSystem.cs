namespace AlvorEngine.Loop;

[Root]
public class RootUiSystem(RootSprites sprites)
{
    public void Size(Vector2 s, EntObj n)
    {
        if (n.TryGetStackedNodeR(out var stackedNode))
            n.Nodes().Remove(stackedNode);

        if (n.NodeStack().TryPeek(out var topStack))
        {
            n.Nodes().Add(topStack);
            n.StackedNodeR() = topStack;
        }

        SizeNode(s, n);
        n.PaddingR() = Get(n.PaddingV(), n.PaddingF());

        foreach (var c in n.Nodes())
            Size(n.SizeR() - n.PaddingR().Xy - n.PaddingR().Zw, c);

        SizeInnerMaxRelative(s, n);
        SizeInnerSumRelative(s, n);
        SizeInnerSizing(s, n);
    }

    private void SizeNode(Vector2 s, EntObj n)
    {
        n.SizeR() = default;
        n.SizeR() += Get(n.SizeRelativeV(), n.SizeRelativeF()) * s;
        n.SizeR() += Get(n.SizeV(), n.SizeF());
        SizeTextRelative(s, n);
    }

    private void SizeTextRelative(Vector2 s, EntObj n)
    {
        if (!n.HasFontV() && !n.HasFontF())
            return;

        var font = Get(n.FontV(), n.FontF());
        if (font == null)
            return;

        var fontSize = Get(n.FontSizeV(), n.FontSizeF());
        if (fontSize <= 0)
            return;

        var text = Get(n.TextV().AsSpan(), n.TextF());
        if (text.IsEmpty)
            return;

        var sizeTextRelative = Get(n.SizeTextRelativeV(), n.SizeTextRelativeF());
        n.SizeR().X += sizeTextRelative.X * sprites.Batch.Measure(font.Size(fontSize), text);
        n.SizeR().Y += sizeTextRelative.Y * font.Size(fontSize).Metrics.Height;
    }

    private void SizeInnerMaxRelative(Vector2 s, EntObj n)
    {
        if (!n.HasSizeInnerMaxRelativeV() && !n.HasSizeInnerMaxRelativeF())
            return;

        var sizeInnerMaxRelative = Get(n.SizeInnerMaxRelativeV(), n.SizeInnerMaxRelativeF());
        var sizeInnerMax = Vector2.Zero;

        foreach (var c in n.Nodes())
        {
            sizeInnerMax.X = Math.Max(c.SizeR().X, sizeInnerMax.X);
            sizeInnerMax.Y = Math.Max(c.SizeR().Y, sizeInnerMax.Y);
        }

        sizeInnerMax.X += n.PaddingR().X + n.PaddingR().Z;
        sizeInnerMax.Y += n.PaddingR().Y + n.PaddingR().W;

        n.SizeR() += sizeInnerMaxRelative * sizeInnerMax;
    }

    private void SizeInnerSumRelative(Vector2 s, EntObj n)
    {
        if (!n.HasSizeInnerSumRelativeV() && !n.HasSizeInnerSumRelativeF())
            return;

        var sizeInnerSumRelative = Get(n.SizeInnerSumRelativeV(), n.SizeInnerSumRelativeF());
        var sizeInnerSum = Vector2.Zero;

        foreach (var c in n.Nodes())
            sizeInnerSum += c.SizeR();

        sizeInnerSum.X += n.PaddingR().X + n.PaddingR().Z;
        sizeInnerSum.Y += n.PaddingR().Y + n.PaddingR().W;

        n.SizeR() += sizeInnerSumRelative * sizeInnerSum;

        var innerSpacing = Get(n.InnerSpacingV(), n.InnerSpacingF());
        n.SizeR() += sizeInnerSumRelative * innerSpacing * Math.Max(0, (n.Nodes().Count - 1));
    }

    private void SizeInnerSizing(Vector2 s, EntObj n)
    {
        if (!n.HasInnerSizingV() && !n.HasInnerSizingF())
            return;

        var innerSizing = Get(n.InnerSizingV(), n.InnerSizingF());
        float totalWeight = 0;

        foreach (var c in n.Nodes())
            totalWeight += Get(n.SizeWeightV(), n.SizeWeightF()) ?? 1;

        var innerSpacing = Get(n.InnerSpacingV(), n.InnerSpacingF());
        var totalSpacing = innerSpacing * Math.Max(0, n.Nodes().Count - 1);
        Vector2 useableSize = n.SizeR() - (totalSpacing, totalSpacing);

        if (innerSizing == InnerSizing.HorizontalWeight)
        {
            foreach (var c in n.Nodes())
                c.SizeR().X = (Get(n.SizeWeightV(), n.SizeWeightF()) ?? 1 / totalWeight) * useableSize.X;
        }
        else if (innerSizing == InnerSizing.VerticalWeight)
        {
            foreach (var c in n.Nodes())
                c.SizeR().Y = (Get(n.SizeWeightV(), n.SizeWeightF()) ?? 1 / totalWeight) * useableSize.Y;
        }
    }

    public void Position(Vector2 s, EntObj n)
    {
        PositionNode(s, n);
        foreach (var c in n.Nodes())
        {
            Position(n.SizeR(), c);
            c.OffsetR() += n.PaddingR().Xy;
        }

        var innerLayout = Get(n.InnerLayoutV(), n.InnerLayoutF());
        var innerSpacing = Get(n.InnerSpacingV(), n.InnerSpacingF());

        if (innerLayout == InnerLayout.VerticalList)
        {
            float y = 0;

            foreach (var c in n.Nodes())
            {
                c.OffsetR().Y += y;
                y += c.SizeR().Y;
                y += innerSpacing;
            }
        }
        else if (innerLayout == InnerLayout.HorizontalList)
        {
            float x = 0;

            foreach (var c in n.Nodes())
            {
                c.OffsetR().X += x;
                x += c.SizeR().X;
                x += innerSpacing;
            }
        }
    }

    public void PositionNode(Vector2 s, EntObj n)
    {
        n.OffsetR() = default;
        n.OffsetR() += Get(n.OffsetV(), n.OffsetF());
        PositionAlignement(s, n);
    }

    private void PositionAlignement(Vector2 s, EntObj n)
    {
        if (!n.HasAlignmentV() && !n.HasAlignmentF())
            return;

        var alignment = Get(n.AlignmentV(), n.AlignmentF());
        Align(ref n.OffsetR(), n.SizeR(), s, alignment);
    }

    public void Draw(Vector2 o, EntObj n)
    {
        DrawNode(o + n.OffsetR(), n);
        foreach (var sc in n.Nodes())
            Draw(o + n.OffsetR(), sc);
    }

    public void DrawNode(Vector2 o, EntObj n)
    {
        DrawFlatSurface(o, n);
        DrawText(o, n);
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

    private void DrawText(Vector2 o, EntObj n)
    {
        if (!n.HasFontV() && !n.HasFontF())
            return;

        var font = Get(n.FontV(), n.FontF());
        if (font == null)
            return;

        var fontSize = Get(n.FontSizeV(), n.FontSizeF());
        if (fontSize <= 0)
            return;

        var text = Get(n.TextV().AsSpan(), n.TextF());
        if (text.IsEmpty)
            return;

        var textColor = Get(n.TextColorV(), n.TextColorF());
        if (textColor.W == 0)
            return;

        var alignment = Get(n.TextAlignmentV(), n.TextAlignmentF()) ?? Alignment.Center;
        var size = new Vector2(sprites.Batch.Measure(font.Size(fontSize), text), font.Size(fontSize).Metrics.Height);
        var offset = Vector2.Zero;

        Align(ref offset, size, n.SizeR(), alignment);
        offset.Y += size.Y / 2;
        sprites.Batch.Write(font.Size(fontSize), text, o + offset, textColor);
    }

    private T? Get<T>(T? value, Func<T>? func) where T : allows ref struct
    {
        if (func != null)
            return func.Invoke();
        else return value;
    }

    private void Align(ref Vector2 val, Vector2 size, Vector2 parent, Alignment alignment)
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
