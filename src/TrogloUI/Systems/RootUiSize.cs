namespace TrogloUI;

[Root]
public class RootUiSize(RootSprites sprites, RootUiScale scale)
{
    internal void Size(Vector2 s, EntMut n)
    {
        SizeNode(s, n);
        SizeInnerSizing(s, n);
        n.PaddingR = Get(n.PaddingV, n.PaddingFDelegate);

        foreach (var c in n.NodesR.Span)
            Size(n.SizeR - n.PaddingR.Xy - n.PaddingR.Zw, c);

        SizeInnerMaxRelative(s, n);
        SizeInnerSumRelative(s, n);

        foreach (var c in n.NodesR.Span)
        {
            if (!c.HasIsPostSizedV && !c.HasIsPostSizedF)
                continue;

            var isPostSized = Get(c.IsPostSizedV, c.IsPostSizedFDelegate);
            if (!isPostSized)
                continue;

            Size(n.SizeR - n.PaddingR.Xy - n.PaddingR.Zw, c);
        }
    }

    private void SizeNode(Vector2 s, EntMut n)
    {
        n.SizeR = default;
        n.SizeR += (Get(n.SizeRelativeV, n.SizeRelativeFDelegate) ?? (1, 1)) * s;
        n.SizeR += Get(n.SizeV, n.SizeFDelegate);
        SizeTextRelative(s, n);

        var hor = n.HorizontalWeightSizeR;
        if (hor != null)
            n.SizeR = n.SizeR with { X = hor.GetValueOrDefault() };

        var ver = n.VerticalWeightSizeR;
        if (ver != null)
            n.SizeR = n.SizeR with { Y = ver.GetValueOrDefault() };
    }

    private void SizeTextRelative(Vector2 s, EntMut n)
    {
        if (!n.HasFontV && !n.HasFontF)
            return;

        var font = Get(n.FontV, n.FontFDelegate);
        if (font == null)
            return;

        var fontSize = (int)(Get(n.FontSizeV, n.FontSizeFDelegate) * scale.Scale);
        if (fontSize <= 0)
            return;

        var text = n.TextFV.Resolve();
        var sizeTextRelative = Get(n.SizeTextRelativeV, n.SizeTextRelativeFDelegate);
        var size = new Vector2(sprites.Batch.Measure(font.Size(fontSize), text) / scale.Scale, font.Size(fontSize).Metrics.Height / scale.Scale);

        var fontPadding = Get(n.FontPaddingV, n.FontPaddingFDelegate);
        var textPadding = Get(n.TextPaddingV, n.TextPaddingFDelegate);

        size += fontPadding.Xy + fontPadding.Zw;
        size += textPadding.Xy + textPadding.Zw;

        n.SizeR += sizeTextRelative * size;
    }

    private void SizeInnerMaxRelative(Vector2 s, EntMut n)
    {
        if (!n.HasSizeInnerMaxRelativeV && !n.HasSizeInnerMaxRelativeF)
            return;

        var sizeInnerMaxRelative = Get(n.SizeInnerMaxRelativeV, n.SizeInnerMaxRelativeFDelegate);
        var sizeInnerMax = Vector2.Zero;

        foreach (var c in n.NodesR.Span)
        {
            if (IsFloating(c))
                continue;

            sizeInnerMax.X = Math.Max(c.SizeR.X, sizeInnerMax.X);
            sizeInnerMax.Y = Math.Max(c.SizeR.Y, sizeInnerMax.Y);
        }

        sizeInnerMax.X += n.PaddingR.X + n.PaddingR.Z;
        sizeInnerMax.Y += n.PaddingR.Y + n.PaddingR.W;

        n.SizeR += sizeInnerMaxRelative * sizeInnerMax;
    }

    private void SizeInnerSumRelative(Vector2 s, EntMut n)
    {
        if (!n.HasSizeInnerSumRelativeV && !n.HasSizeInnerSumRelativeF)
            return;

        var sizeInnerSumRelative = Get(n.SizeInnerSumRelativeV, n.SizeInnerSumRelativeFDelegate);
        var sizeInnerSum = Vector2.Zero;

        foreach (var c in n.NodesR.Span)
        {
            if (IsFloating(c))
                continue;

            sizeInnerSum += c.SizeR;
        }

        sizeInnerSum.X += n.PaddingR.X + n.PaddingR.Z;
        sizeInnerSum.Y += n.PaddingR.Y + n.PaddingR.W;

        var innerSpacing = Get(n.InnerSpacingV, n.InnerSpacingFDelegate);
        var innerSum = sizeInnerSum + new Vector2(innerSpacing) * Math.Max(0, (n.NodesR.Length - 1));
        n.SizeR += sizeInnerSumRelative * innerSum;
        n.SizeInnerSumR = innerSum;
    }

    private void SizeInnerSizing(Vector2 s, EntMut n)
    {
        if (!n.HasInnerSizingV && !n.HasInnerSizingF)
            return;

        var innerSizing = Get(n.InnerSizingV, n.InnerSizingFDelegate);
        float totalWeight = 0;

        foreach (var c in n.NodesR.Span)
        {
            c.UnsetHorizontalWeightSizeR();
            c.UnsetVerticalWeightSizeR();

            if (IsSelfWeight(c))
                continue;

            totalWeight += Get(n.SizeWeightV, n.SizeWeightFDelegate) ?? 1;
        }

        var innerSpacing = Get(n.InnerSpacingV, n.InnerSpacingFDelegate);
        var totalSpacing = innerSpacing * Math.Max(0, n.NodesR.Length - 1);
        Vector2 useableSize = n.SizeR - (totalSpacing, totalSpacing);

        if (innerSizing == InnerSizing.HorizontalWeight)
        {
            foreach (var c in n.NodesR.Span)
            {
                if (IsSelfWeight(c))
                    useableSize.X -= c.SizeR.X;
            }

            foreach (var c in n.NodesR.Span)
            {
                if (IsSelfWeight(c))
                    continue;

                c.Mutate().HorizontalWeightSizeR((Get(n.SizeWeightV, n.SizeWeightFDelegate) ?? 1 / totalWeight) * useableSize.X);
            }
        }
        else if (innerSizing == InnerSizing.VerticalWeight)
        {
            foreach (var c in n.NodesR.Span)
            {
                if (IsSelfWeight(c))
                    useableSize.Y -= c.SizeR.Y;
            }

            foreach (var c in n.NodesR.Span)
            {
                if (IsSelfWeight(c))
                    continue;

                c.Mutate().VerticalWeightSizeR((Get(n.SizeWeightV, n.SizeWeightFDelegate) ?? 1 / totalWeight) * useableSize.Y);
            }
        }
    }

    private bool IsFloating(EntMut n)
    {
        if (!n.HasIsFloatingV && !n.HasIsFloatingF)
            return false;

        return Get(n.IsFloatingV, n.IsFloatingFDelegate);
    }

    private bool IsSelfWeight(EntMut n)
    {
        if (!n.HasSizeWeightTypeV && !n.HasSizeWeightTypeF)
            return false;

        return Get(n.SizeWeightTypeV, n.SizeWeightTypeFDelegate) == SizeWeightType.Self;
    }
}
