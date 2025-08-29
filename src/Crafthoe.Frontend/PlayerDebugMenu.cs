namespace Crafthoe.Frontend;

[Player]
public class PlayerDebugMenu(
    RootGlw gl,
    RootText text,
    RootMetrics metrics,
    AppStyle s,
    DimensionMetrics dimensionMetrics,
    PlayerEnt ent,
    PlayerCamera camera,
    PlayerSelected selected)
{
    public EntObj Get()
    {
        Node(out var menu).SizeRelativeV((1, 1));

        List<Func<ReadOnlySpan<char>>> lines =
        [
            () => text.Format("Frame: {0}. {1:F3} ms ({2} FPS)",
                metrics.Frame.Ticks, metrics.FrameWindow.Average, metrics.FrameWindow.Ticks),
            () => text.Format("Position: {0:F3}", ent.Ent.Position()),
            () => text.Format("Velocity: {0:F3}", ent.Ent.Velocity()),
            () => text.Format("Collision: {0}", ent.Ent.CollisionNormal()),
            () => text.Format("Rotation: {0:F3}", camera.Rotation),
            () => text.Format("Spike: {0}", metrics.Frame.Max),
            () => text.Format("Render: {0}", dimensionMetrics.RenderMetric.Value.Max),
            () => text.Format("Chunk: {0}", dimensionMetrics.ChunkMetric.Value.Max),
            () => text.Format("Section: {0}", dimensionMetrics.SectionMetric.Value.Max),
            () => text.Format("Buffers: {0}", gl.BufferTotalUsage),
            () => text.Format("Selected Loc: {0}", selected.Loc.GetValueOrDefault()),
            () => text.Format("Selected Normal: {0}", selected.Normal.GetValueOrDefault()),
            () => text.Format("TPS: {0}", dimensionMetrics.TickMetricWindow.Value.Ticks)
        ];

        Node(menu, out var list)
            .SizeInnerMaxRelativeV((1, 0))
            .SizeInnerSumRelativeV((0, 1))
            .InnerLayoutV(InnerLayout.VerticalList);
        {
            lines.ForEach(x => Node(list)
                .Mut(s.Label)
                .ColorV((0.5f, 0.5f, 0.5f, 0.5f))
                .TextF(x));
        }

        return menu;
    }
}
