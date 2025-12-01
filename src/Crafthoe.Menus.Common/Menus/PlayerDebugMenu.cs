namespace Crafthoe.Menus.Common;

[Player]
public class PlayerDebugMenu(
    RootGlw gl,
    RootText text,
    RootMetrics metrics,
    RootKeyboard keyboard,
    AppStyle s,
    WorldEntArena entArena,
    PlayerMetrics playerMetrics,
    DimensionSharedVertexBuffer svb,
    DimensionSelected selected,
    PlayerEnt ent,
    PlayerCamera camera)
{
    public void Create(EntObj root)
    {
        List<Func<ReadOnlySpan<char>>> lines =
        [
            () => text.Format("Frame: {0}. {1:F3} ms ({2} FPS)",
                metrics.Frame.Ticks, metrics.FrameWindow.Average, metrics.FrameWindow.Ticks),
            () => text.Format("Position: {0:F3}", ent.Ent.Position()),
            () => text.Format("Velocity: {0:F3}", ent.Ent.Velocity()),
            () => text.Format("Collision: {0}", ent.Ent.CollisionNormal()),
            () => text.Format("Rotation: {0:F3}", camera.Rotation),
            () => text.Format("Spike: {0}", metrics.Frame.Max),
            () => text.Format("Tick: {0}", playerMetrics.TickMetric.Value.Max),
            () => text.Format("Render: {0}", playerMetrics.RenderMetric.Value.Max),
            () => text.Format("Buffers: {0}", gl.BufferTotalUsage),
            () => text.Format("Selected Loc: {0}", selected[(EntMut)ent.Ent].GetValueOrDefault().Loc),
            () => text.Format("Selected Normal: {0}", selected[(EntMut)ent.Ent].GetValueOrDefault().Normal),
            () => text.Format("TPS: {0}", playerMetrics.TickMetricWindow.Value.Ticks),
            () => text.Format("SVB: {0}", svb.Allocator.Used),
            () => text.Format("Arena: {0}", entArena.Allocated),
        ];

        Node(root, out var list)
            .SizeInnerMaxRelativeV(s.Horizontal)
            .SizeInnerSumRelativeV(s.Vertical)
            .InnerLayoutV(InnerLayout.VerticalList);
        {
            lines.ForEach(x => Node(list)
                .Mut(s.Label)
                .ColorV((0.5f, 0.5f, 0.5f, 0.5f))
                .TextF(x));
        }

        Node(root).OnUpdateF(() =>
        {
            if (keyboard.IsKeyPressed(Keys.F3))
                list.IsDisabledV() = !list.IsDisabledV();
        });
    }
}
