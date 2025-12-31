namespace Craftdig.Client;

[Player]
public class PlayerSections(
    DimensionBlocksRaw blocksRaw,
    DimensionBlockChanges blockChanges,
    PlayerSectionUpdateReceiver sectionUpdateReceiver,
    PlayerSectionUpdateQueue sectionUpdateQueue,
    PlayerAheadSections aheadSections)
{
    public void Frame()
    {
        int count = sectionUpdateQueue.Count;
        while (count > 0 && sectionUpdateQueue.TryDequeue(out var item))
        {
            var (sloc, blocks) = item;

            if (!blocksRaw.TryGetChunkBlocks(sloc.Xy, out var chunkBlocks))
                continue;

            var slice = chunkBlocks.Slice(sloc.Z);

            for (int z = 0; z < SectionSize; z++)
            {
                for (int y = 0; y < SectionSize; y++)
                {
                    for (int x = 0; x < SectionSize; x++)
                    {
                        var dt = new Vector3i(x, y, z);
                        var index = dt.ToInnerIndex();

                        if (blocks[index] != slice[index])
                            blockChanges.Add(sloc * SectionSize + dt, slice[index]);

                        slice[index] = blocks[index];
                    }
                }
            }

            sectionUpdateReceiver.Return(blocks);
            aheadSections.Remove(sloc);

            count--;
        }
    }
}
