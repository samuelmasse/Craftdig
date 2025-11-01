namespace Crafthoe.Dimension.Frontend;

[Dimension]
public class DimensionSectionReceiver(
    DimensionSectionMeshTransferer meshTransferer,
    DimensionSections sections,
    DimensionSectionThreadBufferBag bag,
    DimensionSectionThreadOutputBag outputBag)
{
    public void Frame()
    {
        int count = outputBag.Count;

        while (count > 0 && outputBag.TryTake(out var output))
        {
            Receive(output);

            output.Buffer.Clear();
            bag.Add(output.Buffer);

            count--;
        }
    }

    private void Receive(SectionThreadOutput output)
    {
        if (!sections.TryGet(output.Sloc, out var section))
            return;

        meshTransferer.Transfer(
            CollectionsMarshal.AsSpan(output.Buffer),
            ref section.TerrainMesh());

        if (section.TerrainMesh().Count > 0 && !section.Chunk().Rendered().ContainsKey(section.Sloc().Z))
            section.Chunk().Rendered().Add(section.Sloc().Z, section.Sloc().Z);
    }
}
