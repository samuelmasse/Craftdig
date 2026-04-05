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

        var nodes = Nodes(n);
        if (orderBufferKeys.Length <= nodes.Length)
        {
            Array.Resize(ref orderBufferKeys, MathHelper.NextPowerOfTwo(nodes.Length));
            Array.Resize(ref orderBufferVals, MathHelper.NextPowerOfTwo(nodes.Length));
        }

        var keys = orderBufferKeys.AsSpan()[..nodes.Length];
        var vals = orderBufferVals.AsSpan()[..nodes.Length];

        for (int i = 0; i < nodes.Length; i++)
        {
            keys[i] = nodes[i];
            vals[i] = nodes[i].OrderValueFV.Resolve();
        }

        vals.Sort(keys);

        for (int i = 0; i < nodes.Length; i++)
            nodes[i] = keys[i];
    }

    private void RemoveNodes(EntMut n)
    {
        for (int i = NodesCount(n) - 1; i >= 0; i--)
        {
            var c = Nodes(n)[i];

            var isDeleted = c.IsDeletedFV.Resolve();
            if (isDeleted)
                NodesRemoveAt(n, i);
        }
    }

    private void StackNodes(EntMut n)
    {
        var stackedNode = n.StackedNodeR;
        if (stackedNode != default)
            NodesRemove(n, stackedNode);

        if (NodeStackTryPeek(n, out var topStack))
        {
            NodesAdd(n, topStack);
            n.StackedNodeR = topStack;
        }
    }

    private void CompileNodes(EntMut n)
    {
        int start = traverseBufferIndex;
        int count = 0;

        foreach (var c in Nodes(n))
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
