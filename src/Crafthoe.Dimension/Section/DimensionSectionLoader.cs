namespace Crafthoe.Dimension;

[Dimension]
public class DimensionSectionLoader(
    DimensionMetrics metrics,
    DimensionSectionThreadWorkQueue sectionThreadWorkQueue)
{
    public void Load(EntMut section)
    {
        metrics.SectionMetric.Start();

        sectionThreadWorkQueue.Enqeue(section.Sloc());

        section.Chunk().Unrendered().Remove(section.Sloc().Z);

        metrics.SectionMetric.End();
    }
}
