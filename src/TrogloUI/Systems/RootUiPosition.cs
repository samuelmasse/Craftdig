namespace TrogloUI;

[Root]
public class RootUiPosition(RootSprites sprites, RootUiScale scale)
{
    internal void Position(Vector2 s, EntMut n, float? snap)
    {
        PositionNode(s, n, snap);
        var innerSnap = ResolveInnerSnap(n, snap);

        foreach (var c in n.NodesR.Span)
        {
            var ce = c;
            Position(n.SizeR, c, innerSnap);

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

    internal void Finalize(Vector2 o, EntMut n)
    {
        n.PositionR = o + n.OffsetR;

        foreach (var sc in n.NodesR.Span)
            Finalize(n.PositionR, sc);
    }

    private void PositionNode(Vector2 s, EntMut n, float? snap)
    {
        n.OffsetR = default;
        n.OffsetR += n.OffsetFV.Resolve();
        PositionTextRelative(n);
        PositionAlignement(s, n, snap);
    }

    private void PositionAlignement(Vector2 s, EntMut n, float? snap)
    {
        var alignment = n.AlignmentFV.Resolve();
        var snapValue = n.AlignmentSnapFV.Resolve() ?? snap ?? 0;
        n.OffsetR = Align(n.OffsetR, n.SizeR, s, alignment, snapValue);
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

    internal Vector2 Align(Vector2 val, Vector2 size, Vector2 parent, Alignment alignment, float snap)
    {
        if ((alignment & Alignment.Horizontal) != 0)
            val.X += Snap(parent.X / 2, snap) - size.X / 2;
        if ((alignment & Alignment.Vertical) != 0)
            val.Y += Snap(parent.Y / 2, snap) - size.Y / 2;

        if ((alignment & Alignment.Right) != 0)
            val.X += parent.X - size.X;
        if ((alignment & Alignment.Bottom) != 0)
            val.Y += parent.Y - size.Y;

        return val;
    }

    private static float? ResolveInnerSnap(EntMut n, float? snap)
    {
        // If the node explicitly defines an inner snap, carry that forward to children
        var innerSnap = n.InnerAlignmentSnapFV.Resolve();
        if (innerSnap != null)
            return innerSnap;

        // If this node has centering alignment, the snap is consumed by this node and not passed down
        var aligned = (n.AlignmentFV.Resolve() & (Alignment.Horizontal | Alignment.Vertical)) != 0;
        if (aligned)
            return null;

        // Otherwise pass down the inherited snap to children until first consumer is reached
        return snap;
    }

    private float Snap(float value, float snap) => snap > 0 ? (float)Math.Round(value / snap) * snap : value;
}
