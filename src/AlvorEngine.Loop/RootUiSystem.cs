namespace AlvorEngine.Loop;

[Root]
public class RootUiSystem(RootScale rscale, RootSprites sprites)
{
    private float scale = rscale.Scale;

    private EntObj[] traverseBuffer = new EntObj[16];
    private int traverseBufferIndex;

    private EntObj[] orderBufferKeys = new EntObj[16];
    private float[] orderBufferVals = new float[16];

    public float Scale
    {
        get => scale;
        set => scale = value;
    }

    public void Traverse(EntObj n, int depth)
    {
        if (depth == 0)
            traverseBufferIndex = 0;

        RemoveNodes(n);
        OrderNodes(n);
        StackNodes(n);
        CompileNodes(n);

        foreach (var c in n.GetNodesR())
            Traverse(c, depth + 1);
    }

    private void OrderNodes(EntObj n)
    {
        if (!n.HasIsOrderedV() && !n.HasIsOrderedF())
            return;

        var ordered = Get(n.IsOrderedV(), n.IsOrderedF());
        if (!ordered)
            return;

        var nodes = n.Nodes();
        if (orderBufferKeys.Length <= nodes.Count)
        {
            Array.Resize(ref orderBufferKeys, MathHelper.NextPowerOfTwo(nodes.Count));
            Array.Resize(ref orderBufferVals, MathHelper.NextPowerOfTwo(nodes.Count));
        }

        var keys = orderBufferKeys.AsSpan()[..nodes.Count];
        var vals = orderBufferVals.AsSpan()[..nodes.Count];

        for (int i = 0; i < nodes.Count; i++)
        {
            keys[i] = nodes[i];
            vals[i] = Get(nodes[i].OrderValueV(), nodes[i].OrderValueF());
        }

        vals.Sort(keys);

        for (int i = 0; i < nodes.Count; i++)
            nodes[i] = keys[i];
    }

    private void RemoveNodes(EntObj n)
    {
        for (int i = n.Nodes().Count - 1; i >= 0; i--)
        {
            var c = n.Nodes()[i];
            if (!c.HasIsDeletedV() && !c.HasIsDeletedF())
                continue;

            var isDeleted = Get(c.IsDeletedV(), c.IsDeletedF());
            if (isDeleted)
                n.Nodes().RemoveAt(i);
        }
    }

    private void StackNodes(EntObj n)
    {
        if (n.TryGetStackedNodeR(out var stackedNode))
            n.Nodes().Remove(stackedNode);

        if (n.NodeStack().TryPeek(out var topStack))
        {
            n.Nodes().Add(topStack);
            n.StackedNodeR() = topStack;
        }
    }

    private void CompileNodes(EntObj n)
    {
        int start = traverseBufferIndex;
        int count = 0;

        foreach (var c in n.Nodes())
        {
            var disabled = Get(c.IsDisabledV(), c.IsDisabledF());
            if (disabled)
                continue;

            if (traverseBufferIndex == traverseBuffer.Length)
                Array.Resize(ref traverseBuffer, traverseBuffer.Length * 2);
            traverseBuffer[traverseBufferIndex++] = c;
            count++;
        }

        n.NodesR() = traverseBuffer.AsMemory().Slice(start, count);
    }

    public void Size(Vector2 s, EntObj n)
    {
        SizeNode(s, n);
        n.PaddingR() = Get(n.PaddingV(), n.PaddingF());

        foreach (var c in n.GetNodesR())
            Size(n.SizeR() - n.PaddingR().Xy - n.PaddingR().Zw, c);

        SizeInnerMaxRelative(s, n);
        SizeInnerSumRelative(s, n);
        SizeInnerSizing(s, n);
    }

    private void SizeNode(Vector2 s, EntObj n)
    {
        n.SizeR() = default;
        n.SizeR() += (Get(n.SizeRelativeV(), n.SizeRelativeF()) ?? (1, 1)) * s;
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

        var fontSize = (int)(Get(n.FontSizeV(), n.FontSizeF()) * scale);
        if (fontSize <= 0)
            return;

        var text = Get(n.TextV().AsSpan(), n.TextF());
        if (text.IsEmpty)
            return;

        var sizeTextRelative = Get(n.SizeTextRelativeV(), n.SizeTextRelativeF());
        n.SizeR().X += sizeTextRelative.X * (sprites.Batch.Measure(font.Size(fontSize), text) / scale);
        n.SizeR().Y += sizeTextRelative.Y * (font.Size(fontSize).Metrics.Height / scale);
    }

    private void SizeInnerMaxRelative(Vector2 s, EntObj n)
    {
        if (!n.HasSizeInnerMaxRelativeV() && !n.HasSizeInnerMaxRelativeF())
            return;

        var sizeInnerMaxRelative = Get(n.SizeInnerMaxRelativeV(), n.SizeInnerMaxRelativeF());
        var sizeInnerMax = Vector2.Zero;

        foreach (var c in n.GetNodesR())
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

        foreach (var c in n.GetNodesR())
            sizeInnerSum += c.SizeR();

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
            totalWeight += Get(n.SizeWeightV(), n.SizeWeightF()) ?? 1;

        var innerSpacing = Get(n.InnerSpacingV(), n.InnerSpacingF());
        var totalSpacing = innerSpacing * Math.Max(0, n.GetNodesR().Length - 1);
        Vector2 useableSize = n.SizeR() - (totalSpacing, totalSpacing);

        if (innerSizing == InnerSizing.HorizontalWeight)
        {
            foreach (var c in n.GetNodesR())
                c.SizeR().X = (Get(n.SizeWeightV(), n.SizeWeightF()) ?? 1 / totalWeight) * useableSize.X;
        }
        else if (innerSizing == InnerSizing.VerticalWeight)
        {
            foreach (var c in n.GetNodesR())
                c.SizeR().Y = (Get(n.SizeWeightV(), n.SizeWeightF()) ?? 1 / totalWeight) * useableSize.Y;
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
        PositionAlignement(s, n);
    }

    private void PositionAlignement(Vector2 s, EntObj n)
    {
        var alignment = GetAlignment(n);
        Align(ref n.OffsetR(), n.SizeR(), s, alignment);
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

        var fontSize = (int)(Get(n.FontSizeV(), n.FontSizeF()) * scale);
        if (fontSize <= 0)
            return;

        var text = Get(n.TextV().AsSpan(), n.TextF());
        if (text.IsEmpty)
            return;

        var textColor = Get(n.TextColorV(), n.TextColorF());
        if (textColor.W == 0)
            return;

        var alignment = Get(n.TextAlignmentV(), n.TextAlignmentF()) ?? Alignment.Center;
        var size = new Vector2(sprites.Batch.Measure(font.Size(fontSize), text), font.Size(fontSize).Metrics.Height) / scale;
        var offset = Vector2.Zero;

        Align(ref offset, size, n.SizeR(), alignment);
        offset.Y += size.Y / 2;
        sprites.Batch.Write(font.Size(fontSize), text, (o + offset) * scale, textColor, scale);
    }

    public T? Get<T>(T? value, Func<T>? func) where T : allows ref struct
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
