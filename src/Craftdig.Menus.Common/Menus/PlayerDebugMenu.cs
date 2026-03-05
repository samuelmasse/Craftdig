namespace Craftdig.Menus.Common;

[Player]
public class PlayerDebugMenu(
    RootGlw gl,
    RootText text,
    RootMetrics metrics,
    RootKeyboard keyboard,
    AppStyle s,
    WorldEntArena worldEntArena,
    DimensionSharedVertexBuffer svb,
    DimensionSelected selected,
    DimensionEntArena dimensionEntArena,
    PlayerMetrics playerMetrics,
    PlayerEnt ent,
    PlayerCamera camera)
{
    public void Create(EntObj root)
    {
        var gpu = GL.GetString(StringName.Renderer);

        List<Func<ReadOnlySpan<char>>> lines =
        [
            () => text.Format("Frame: {0}. {1:F3} ms ({2} FPS)",
                metrics.Frame.Ticks, metrics.FrameWindow.Average, metrics.FrameWindow.Ticks),
            () => text.Format("GPU: {0}", gpu),
            () => text.Format("Position: {0:F3}", ent.Position),
            () => text.Format("Velocity: {0:F3}", ent.Velocity),
            () => text.Format("Collision: {0}", ent.CollisionNormal),
            () => text.Format("Rotation: {0:F3}", camera.Rotation),
            () => text.Format("Spike: {0}", metrics.Frame.Max),
            () => text.Format("Tick: {0}", playerMetrics.TickMetric.Value.Max),
            () => text.Format("Render: {0}", playerMetrics.RenderMetric.Value.Max),
            () => text.Format("Buffers: {0}", gl.BufferTotalUsage),
            () => text.Format("Selected Loc: {0}", selected[ent].GetValueOrDefault().Loc),
            () => text.Format("Selected Normal: {0}", selected[ent].GetValueOrDefault().Normal),
            () => text.Format("TPS: {0}", playerMetrics.TickMetricWindow.Value.Ticks),
            () => text.Format("SVB: {0}", svb.Allocator.Used),
            () => text.Format("World Arena: {0}", worldEntArena.Arena.Allocated),
            () => text.Format("Dimension Arena: {0}", dimensionEntArena.Arena.Allocated),
        ];

        Node(root, out var list)
            .SizeInnerMaxRelativeV(s.Horizontal)
            .SizeInnerSumRelativeV(s.Vertical)
            .InnerLayoutV(InnerLayout.VerticalList);
        {
            lines.ForEach(x => Node(list)
                .Mutate(s.Label)
                .ColorV((0.5f, 0.5f, 0.5f, 0.5f))
                .TextF(x));
        }

        Node(root).OnUpdateF(() =>
        {
            if (keyboard.IsKeyPressed(Keys.F3))
                list.IsDisabledV = !list.IsDisabledV;
        });
    }
}
