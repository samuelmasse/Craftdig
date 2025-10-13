namespace TrogloUI;

[Root]
public class RootUiSystem(RootSprites sprites, RootUiScale scale)
{
    public void Size(Vector2 s, EntObj n)
    {
        SizeNode(s, n);
        SizeInnerSizing(s, n);
        n.PaddingR() = Get(n.PaddingV(), n.PaddingF());

        foreach (var c in n.GetNodesR())
            Size(n.SizeR() - n.PaddingR().Xy - n.PaddingR().Zw, c);

        SizeInnerMaxRelative(s, n);
        SizeInnerSumRelative(s, n);

        foreach (var c in n.GetNodesR())
        {
            if (!c.HasIsPostSizedV() && !c.HasIsPostSizedF())
                continue;

            var isPostSized = Get(c.IsPostSizedV(), c.IsPostSizedF());
            if (!isPostSized)
                continue;

            Size(n.SizeR() - n.PaddingR().Xy - n.PaddingR().Zw, c);
        }
    }

    private void SizeNode(Vector2 s, EntObj n)
    {
        n.SizeR() = default;
        n.SizeR() += (Get(n.SizeRelativeV(), n.SizeRelativeF()) ?? (1, 1)) * s;
        n.SizeR() += Get(n.SizeV(), n.SizeF());
        SizeTextRelative(s, n);

        var hor = n.GetHorizontalWeightSizeR();
        if (hor != null)
            n.SizeR().X = hor.GetValueOrDefault();

        var ver = n.GetVerticalWeightSizeR();
        if (ver != null)
            n.SizeR().Y = ver.GetValueOrDefault();
    }

    private void SizeTextRelative(Vector2 s, EntObj n)
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
        var sizeTextRelative = Get(n.SizeTextRelativeV(), n.SizeTextRelativeF());
        var size = new Vector2(sprites.Batch.Measure(font.Size(fontSize), text) / scale.Scale, font.Size(fontSize).Metrics.Height / scale.Scale);

        var fontPadding = Get(n.FontPaddingV(), n.FontPaddingF());
        var textPadding = Get(n.TextPaddingV(), n.TextPaddingF());

        size += fontPadding.Xy + fontPadding.Zw;
        size += textPadding.Xy + textPadding.Zw;

        n.SizeR() += sizeTextRelative * size;
    }

    private void SizeInnerMaxRelative(Vector2 s, EntObj n)
    {
        if (!n.HasSizeInnerMaxRelativeV() && !n.HasSizeInnerMaxRelativeF())
            return;

        var sizeInnerMaxRelative = Get(n.SizeInnerMaxRelativeV(), n.SizeInnerMaxRelativeF());
        var sizeInnerMax = Vector2.Zero;

        foreach (var c in n.GetNodesR())
        {
            if (IsFloating(c))
                continue;

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

        foreach (var c in n.GetNodesR())
        {
            if (IsFloating(c))
                continue;

            sizeInnerSum += c.SizeR();
        }

        sizeInnerSum.X += n.PaddingR().X + n.PaddingR().Z;
        sizeInnerSum.Y += n.PaddingR().Y + n.PaddingR().W;

        n.SizeR() += sizeInnerSumRelative * sizeInnerSum;

        var innerSpacing = Get(n.InnerSpacingV(), n.InnerSpacingF());
        n.SizeR() += sizeInnerSumRelative * innerSpacing * Math.Max(0, (n.GetNodesR().Length - 1));
    }

    private void SizeInnerSizing(Vector2 s, EntObj n)
    {
        if (!n.HasInnerSizingV() && !n.HasInnerSizingF())
            return;

        var innerSizing = Get(n.InnerSizingV(), n.InnerSizingF());
        float totalWeight = 0;

        foreach (var c in n.GetNodesR())
        {
            c.UnsetHorizontalWeightSizeR();
            c.UnsetVerticalWeightSizeR();

            if (IsSelfWeight(c))
                continue;

            totalWeight += Get(n.SizeWeightV(), n.SizeWeightF()) ?? 1;
        }

        var innerSpacing = Get(n.InnerSpacingV(), n.InnerSpacingF());
        var totalSpacing = innerSpacing * Math.Max(0, n.GetNodesR().Length - 1);
        Vector2 useableSize = n.SizeR() - (totalSpacing, totalSpacing);

        if (innerSizing == InnerSizing.HorizontalWeight)
        {
            foreach (var c in n.GetNodesR())
            {
                if (IsSelfWeight(c))
                    useableSize.X -= c.SizeR().X;
            }

            foreach (var c in n.GetNodesR())
            {
                if (IsSelfWeight(c))
                    continue;

                c.HorizontalWeightSizeR() = (Get(n.SizeWeightV(), n.SizeWeightF()) ?? 1 / totalWeight) * useableSize.X;
            }
        }
        else if (innerSizing == InnerSizing.VerticalWeight)
        {
            foreach (var c in n.GetNodesR())
            {
                if (IsSelfWeight(c))
                    useableSize.Y -= c.SizeR().Y;
            }

            foreach (var c in n.GetNodesR())
            {
                if (IsSelfWeight(c))
                    continue;

                c.VerticalWeightSizeR() = (Get(n.SizeWeightV(), n.SizeWeightF()) ?? 1 / totalWeight) * useableSize.Y;
            }
        }
    }

    public void Position(Vector2 s, EntObj n)
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

        var textColor = Get(n.TextColorV(), n.TextColorF());
        if (textColor.W == 0)
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

    public void Update(EntObj n)
    {
        if (n.HasOnUpdateF())
            n.OnUpdateF()?.Invoke();

        foreach (var c in n.GetNodesR())
            Update(c);
    }

    public void Draw(Vector2 o, EntObj n)
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

        Align(ref offset, size, n.SizeR(), alignment);
        offset.Y += size.Y / 2;

        sprites.Batch.Write(font.Size(fontSize), text, (o + offset) * scale.Scale, textColor, scale.Scale);
    }

    private bool IsFloating(EntObj n)
    {
        if (!n.HasIsFloatingV() && !n.HasIsFloatingF())
            return false;

        return Get(n.IsFloatingV(), n.IsFloatingF());
    }

    private bool IsSelfWeight(EntObj n)
    {
        if (!n.HasSizeWeightTypeV() && !n.HasSizeWeightTypeF())
            return false;

        return Get(n.SizeWeightTypeV(), n.SizeWeightTypeF()) == SizeWeightType.Self;
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
