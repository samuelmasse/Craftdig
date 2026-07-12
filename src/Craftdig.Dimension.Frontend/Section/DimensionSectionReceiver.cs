namespace Craftdig.Dimension.Frontend;

[Dimension]
public class DimensionSectionReceiver(
    DimensionSectionMeshTransferer meshTransferer,
    DimensionSections sections,
    DimensionSectionThreadBufferBag bag,
    DimensionSectionThreadOutputBag outputBag)
{
    private readonly Stopwatch watch = new();

    public void Frame()
    {
        int count = outputBag.Count;
        int received = 0;
        watch.Restart();

        while (count > 0 &&
               (received == 0 || watch.Elapsed.TotalMilliseconds < 2) &&
               outputBag.TryTake(out var output))
        {
            Receive(output);

            output.Buffer.Clear();
            bag.Add(output.Buffer);

            count--;
            received++;
        }
    }

    private void Receive(SectionThreadOutput output)
    {
        if (!sections.TryGet(output.Sloc, out var section))
            return;

        if (output.Revision != section.MeshRevision)
            return;

        section.IsMeshPending = false;
        section.TerrainMesh = meshTransferer.Transfer(
            CollectionsMarshal.AsSpan(output.Buffer),
            section.TerrainMesh);

        if (section.TerrainMesh.Count > 0 && !section.Chunk.Rendered.ContainsKey(section.Sloc.Z))
            section.Chunk.Rendered.Add(section.Sloc.Z, section.Sloc.Z);

        if (section.IsMeshDirty)
        {
            section.IsMeshDirty = false;
            if (section.Chunk.IsReadyToRender && !section.Chunk.Unrendered.ContainsKey(section.Sloc.Z))
                section.Chunk.Unrendered.Add(section.Sloc.Z, section.Sloc.Z);
        }

    }
}
