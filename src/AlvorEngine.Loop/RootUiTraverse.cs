namespace AlvorEngine.Loop;

[Root]
public class RootUiTraverse
{
    private EntObj[] traverseBuffer = new EntObj[16];
    private int traverseBufferIndex;

    private EntObj[] orderBufferKeys = new EntObj[16];
    private float[] orderBufferVals = new float[16];

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
}
