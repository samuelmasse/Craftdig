namespace TrogloUI;

[Root]
public class RootUiSize(RootSprites sprites, RootUiScale scale)
{
    internal void Size(Vector2 s, EntObj n)
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
}
