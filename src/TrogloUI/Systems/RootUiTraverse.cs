namespace TrogloUI;

[Root]
public class RootUiTraverse
{
    private EntMut[] traverseBuffer = new EntMut[16];
    private int traverseBufferIndex;

    private EntMut[] orderBufferKeys = new EntMut[16];
    private float[] orderBufferVals = new float[16];

    internal void Traverse(EntMut n, int depth)
    {
        if (depth == 0)
            traverseBufferIndex = 0;

        if (!n.HasNodes)
            return;

        RemoveNodes(n);
        OrderNodes(n);
        StackNodes(n);
        CompileNodes(n);

        foreach (var c in n.NodesR.Span)
            Traverse(c, depth + 1);
    }

    private void OrderNodes(EntMut n)
    {
        var ordered = n.IsOrderedFV.Resolve();
        if (!ordered)
            return;

        var nodes = n.Nodes;
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
            vals[i] = nodes[i].OrderValueFV.Resolve();
        }

        vals.Sort(keys);

        for (int i = 0; i < nodes.Count; i++)
            nodes[i] = keys[i];
    }

    private void RemoveNodes(EntMut n)
    {
        for (int i = n.Nodes.Count - 1; i >= 0; i--)
        {
            var c = n.Nodes[i];

            var isDeleted = c.IsDeletedFV.Resolve();
            if (isDeleted)
                n.Nodes.RemoveAt(i);
        }
    }

    private void StackNodes(EntMut n)
    {
        var stackedNode = n.StackedNodeR;
        if (stackedNode != default)
            n.Nodes.Remove(stackedNode);

        if (n.HasNodeStack && n.NodeStack.TryPeek(out var topStack))
        {
            n.Nodes.Add(topStack);
            n.StackedNodeR = topStack;
        }
    }

    private void CompileNodes(EntMut n)
    {
        int start = traverseBufferIndex;
        int count = 0;

        foreach (var c in n.Nodes)
        {
            var disabled = c.IsDisabledFV.Resolve();
            if (disabled)
                continue;

            if (traverseBufferIndex == traverseBuffer.Length)
                Array.Resize(ref traverseBuffer, traverseBuffer.Length * 2);
            traverseBuffer[traverseBufferIndex++] = c;
            count++;
        }

        n.NodesR = traverseBuffer.AsMemory().Slice(start, count);
    }
}
