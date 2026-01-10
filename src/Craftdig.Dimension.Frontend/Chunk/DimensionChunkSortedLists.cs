namespace Craftdig.Dimension.Frontend;

public class DimensionChunkSortedLists
{
    private readonly Queue<SortedList<int, int>> queue = [];

    public SortedList<int, int> Take()
    {
        if (queue.Count > 0)
            return queue.Dequeue();

        return [];
    }

    public void Return(SortedList<int, int> list)
    {
        list.Clear();
        queue.Enqueue(list);
    }
}
