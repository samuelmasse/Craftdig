namespace TrogloUI;

[Root]
public class RootUiSize(RootSprites sprites, RootUiScale scale)
{
    internal void Size(Vector2 s, EntMut n)
    {
        SizeNode(s, n);
        n.PaddingR = n.PaddingFV.Resolve();

        foreach (var c in n.NodesR.Span)
        {
            var ce = c;
            ce.MarginR = c.MarginFV.Resolve();
        }

        SizeInnerSizing(s, n);

        var innerSpace = n.SizeR - n.PaddingR.Xy - n.PaddingR.Zw;
        foreach (var c in n.NodesR.Span)
            Size(innerSpace - c.MarginR.Xy - c.MarginR.Zw, c);

        SizeInnerMaxRelative(s, n);
        SizeInnerSumRelative(s, n);

        var sizeSnap = n.SizeAlignmentSnapFV.Resolve();
        n.SizeR = (Snap.Ceiling(n.SizeR.X, sizeSnap), Snap.Ceiling(n.SizeR.Y, sizeSnap));

        var postInnerSpace = n.SizeR - n.PaddingR.Xy - n.PaddingR.Zw;
        foreach (var c in n.NodesR.Span)
        {
            if (!c.IsPostSizedFV.Resolve())
                continue;

            Size(postInnerSpace - c.MarginR.Xy - c.MarginR.Zw, c);
        }
    }

    private void SizeNode(Vector2 s, EntMut n)
    {
        n.SizeR = default;
        n.SizeR += (n.SizeRelativeFV.Resolve() ?? (1, 1)) * s;
        n.SizeR += n.SizeFV.Resolve();
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
        var font = n.FontFV.Resolve();
        if (font == null)
            return;

        var fontSize = (int)(n.FontSizeFV.Resolve() * scale.Scale);
        if (fontSize <= 0)
            return;

        var text = n.TextFV.Resolve();
        var sizeTextRelative = n.SizeTextRelativeFV.Resolve();
        var glyphSnap = n.TextGlyphAlignmentSnapFV.Resolve() * scale.Scale;
        var size = new Vector2(
            sprites.Batch.Measure(font.Size(fontSize), text, glyphSnap) / scale.Scale,
            font.Size(fontSize).Metrics.Height / scale.Scale);

        var fontPadding = n.FontPaddingFV.Resolve();
        var textPadding = n.TextPaddingFV.Resolve();

        size += fontPadding.Xy + fontPadding.Zw;
        size += textPadding.Xy + textPadding.Zw;

        n.SizeR += sizeTextRelative * size;
    }

    private void SizeInnerMaxRelative(Vector2 s, EntMut n)
    {
        var sizeInnerMaxRelative = n.SizeInnerMaxRelativeFV.Resolve();
        if (sizeInnerMaxRelative == default)
            return;

        var sizeInnerMax = Vector2.Zero;

        foreach (var c in n.NodesR.Span)
        {
            if (c.IsFloatingFV.Resolve())
                continue;

            sizeInnerMax.X = Math.Max(c.SizeR.X + c.MarginR.X + c.MarginR.Z, sizeInnerMax.X);
            sizeInnerMax.Y = Math.Max(c.SizeR.Y + c.MarginR.Y + c.MarginR.W, sizeInnerMax.Y);
        }

        sizeInnerMax.X += n.PaddingR.X + n.PaddingR.Z;
        sizeInnerMax.Y += n.PaddingR.Y + n.PaddingR.W;

        n.SizeR += sizeInnerMaxRelative * sizeInnerMax;
    }

    private void SizeInnerSumRelative(Vector2 s, EntMut n)
    {
        var sizeInnerSumRelative = n.SizeInnerSumRelativeFV.Resolve();
        if (sizeInnerSumRelative == default)
            return;

        var sizeInnerSum = Vector2.Zero;

        foreach (var c in n.NodesR.Span)
        {
            if (c.IsFloatingFV.Resolve())
                continue;

            sizeInnerSum += c.SizeR + c.MarginR.Xy + c.MarginR.Zw;
        }

        sizeInnerSum.X += n.PaddingR.X + n.PaddingR.Z;
        sizeInnerSum.Y += n.PaddingR.Y + n.PaddingR.W;

        var innerSpacing = n.InnerSpacingFV.Resolve();
        var innerSum = sizeInnerSum + new Vector2(innerSpacing) * Math.Max(0, (n.NodesR.Length - 1));
        n.SizeR += sizeInnerSumRelative * innerSum;
        n.SizeInnerSumR = innerSum;
    }

    private void SizeInnerSizing(Vector2 s, EntMut n)
    {
        var innerSizing = n.InnerSizingFV.Resolve();
        if (innerSizing == default)
            return;

        float totalWeight = 0;

        foreach (var c in n.NodesR.Span)
        {
            var ce = c;
            ce.HorizontalWeightSizeR = null;
            ce.VerticalWeightSizeR = null;

            if (IsSelfWeight(c))
                continue;

            totalWeight += n.SizeWeightFV.Resolve() ?? 1;
        }

        var innerSpacing = n.InnerSpacingFV.Resolve();
        var totalSpacing = innerSpacing * Math.Max(0, n.NodesR.Length - 1);
        Vector2 useableSize = n.SizeR - (totalSpacing, totalSpacing);

        if (innerSizing == InnerSizing.HorizontalWeight)
        {
            foreach (var c in n.NodesR.Span)
            {
                useableSize.X -= c.MarginR.X + c.MarginR.Z;

                if (IsSelfWeight(c))
                    useableSize.X -= c.SizeR.X;
            }

            foreach (var c in n.NodesR.Span)
            {
                if (IsSelfWeight(c))
                    continue;

                var ce = c;
                ce.HorizontalWeightSizeR = (n.SizeWeightFV.Resolve() ?? 1 / totalWeight) * useableSize.X;
            }
        }
        else if (innerSizing == InnerSizing.VerticalWeight)
        {
            foreach (var c in n.NodesR.Span)
            {
                useableSize.Y -= c.MarginR.Y + c.MarginR.W;

                if (IsSelfWeight(c))
                    useableSize.Y -= c.SizeR.Y;
            }

            foreach (var c in n.NodesR.Span)
            {
                if (IsSelfWeight(c))
                    continue;

                var ce = c;
                ce.VerticalWeightSizeR = (n.SizeWeightFV.Resolve() ?? 1 / totalWeight) * useableSize.Y;
            }
        }
    }

    private bool IsSelfWeight(EntMut n) => n.SizeWeightTypeFV.Resolve() == SizeWeightType.Self;
}
