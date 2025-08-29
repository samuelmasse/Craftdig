namespace Crafthoe.Frontend;

[Player]
public class PlayerState(
    RootCanvas canvas,
    RootMouse mouse,
    RootKeyboard keyboard,
    RootSprites sprites,
    RootScale scale,
    RootUi ui,
    WorldTick tick,
    DimensionMetrics dimensionMetrics,
    DimensionContext dimension,
    PlayerContext player,
    PlayerDebugMenu debugMenu,
    PlayerEscapeMenu escapeMenu) : State
{
    private readonly EntObj menus = Node(ui).SizeRelativeV((1, 1));
    private readonly EntObj dark = Node().SizeRelativeV((1, 1)).ColorV((0.3f, 0.3f, 0.3f, 0.3f));
    private bool paused;

    public override void Load()
    {
        player.Load();
        menus.Nodes().Add(debugMenu.Get());
    }

    public override void Unload()
    {
        ui.Nodes().Remove(menus);
    }

    public override void Update(double time)
    {
        if (keyboard.IsKeyPressed(Keys.Escape))
        {
            if (menus.NodeStack().Count > 0)
                menus.NodeStack().Pop();
            else
            {
                paused = true;
                menus.Nodes().Add(dark);
                menus.NodeStack().Push(escapeMenu.Get(menus));
            }
        }

        if (paused && menus.NodeStack().Count == 0)
        {
            paused = false;
            menus.Nodes().Remove(dark);
        }

        mouse.Track = false;
        if (!paused)
        {
            int ticks = tick.Update(time);
            while (ticks > 0)
            {
                dimensionMetrics.TickMetric.Start();

                player.Tick();
                dimension.Tick();
                ticks--;

                dimensionMetrics.TickMetric.End();
            }

            player.Update(time);
        }

        mouse.CursorState = mouse.Track ? CursorState.Grabbed : CursorState.Normal;
    }

    public override void Render()
    {
        dimension.Frame();
        player.Render();
    }

    public override void Draw()
    {
        float cht = scale[4];
        float chl = cht * 9;
        var c = canvas.Size / 2;

        sprites.Batch.Draw(c - (cht / 2, chl / 2), (cht, chl));
        sprites.Batch.Draw(c - (chl / 2, cht / 2), (chl, cht));
    }
}
