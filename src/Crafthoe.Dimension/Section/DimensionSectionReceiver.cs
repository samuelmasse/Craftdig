namespace Crafthoe.Dimension;

[Dimension]
public class DimensionSectionReceiver(
    DimensionBlockProgram blockProgram,
    DimensionSectionMeshTransferer meshTransferer,
    DimensionSections sections,
    DimensionSectionThreadBufferBag bag,
    DimensionSectionThreadOutputQueue outputQueue)
{
    public void Frame()
    {
        while (outputQueue.TryDequeue(out var output))
        {
            Receive(output);

            output.Buffer.Clear();
            bag.Add(output.Buffer); 
        }
    }

    private void Receive(SectionThreadOutput output)
    {
        if (!sections.TryGet(output.Sloc, out var section))
            return;

        meshTransferer.Transfer(
            blockProgram,
            (ReadOnlySpan<BlockVertex>)CollectionsMarshal.AsSpan(output.Buffer),
            ref section.TerrainMesh());

        if (section.TerrainMesh().Count > 0 && !section.Chunk().Rendered().ContainsKey(section.Sloc().Z))
            section.Chunk().Rendered().Add(section.Sloc().Z, section.Sloc().Z);
    }
}
